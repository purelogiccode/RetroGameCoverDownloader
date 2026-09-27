# Architecture

This page describes how Retro Game Cover Downloader is structured, how a cover download flows through the
system, and which strategies keep it resilient on a rate-limited public API.

## Solution layout

```text
CSharp_RetroGameCoverDownloader.sln
├── RetroGameCoverDownloader/            # WPF desktop application (net10.0-windows)
│   ├── Commands/                        # RelayCommand (ICommand implementation)
│   ├── Converters/                      # XAML value converters
│   ├── Helpers/                         # AppInfo, LogContext, RetryHelper
│   ├── Managers/                        # SettingsManager
│   ├── Models/                          # AppSettings, RetrySettings, SystemConfig, ...
│   ├── Services/                        # GitHub, logging, bug reports, updates, telemetry
│   ├── ViewModels/                      # MainViewModel (MVVM)
│   ├── Views/                           # Dialogs (token, proxy, extensions, about)
│   └── MainWindow.xaml                  # Main window (FluentWindow)
├── RetroGameCoverDownloader.Tests/      # xUnit unit test suite
├── docs/                                # This documentation (DocFX source)
├── scripts/                             # Release packaging and docs tooling
└── .github/workflows/                   # CI, release, and docs pipelines
```

The application follows the **MVVM** pattern for the main window. `MainViewModel` owns all business logic;
dialogs use code-behind for simplicity. The test project references the application and replaces
`BugReportService` with an in-memory mock through a module initializer, so tests never call external APIs.

## High-level view

```text
┌──────────────────────────────────────────────────────────────────────┐
│                          MainWindow (WPF-UI)                         │
│  menus · folder pickers · system list · progress · log view          │
└───────────────────────────────┬──────────────────────────────────────┘
                                │ data binding + ICommand
┌───────────────────────────────▼──────────────────────────────────────┐
│                            MainViewModel                             │
│  scanning · matching · download loop · cancellation · dispatch       │
└──────┬───────────────┬───────────────┬───────────────┬───────────────┘
       │               │               │               │
┌──────▼──────┐ ┌──────▼──────┐ ┌──────▼───────┐ ┌─────▼──────────┐
│GitHubService│ │RateLimiter  │ │RetryHelper   │ │SettingsManager │
│ tree/raw/   │ │ sliding     │ │ backoff +    │ │ DPAPI-encrypted│
│ contents API│ │ 1-hour window│ │ transient   │ │ settings.dat   │
└──────┬──────┘ └─────────────┘ │ detection    │ └────────────────┘
       │                        └──────────────┘
       │ HTTPS
┌──────▼───────────────────────────────────────────────────────────────┐
│ GitHub: api.github.com · raw.githubusercontent.com                   │
│ libretro-thumbnails/libretro-thumbnails (systems, .gitmodules, trees)│
└──────────────────────────────────────────────────────────────────────┘
```

## Components

| Component | File | Responsibility |
| --- | --- | --- |
| `MainViewModel` | `ViewModels/MainViewModel.cs` | Orchestrates scanning, matching, downloading, commands, cancellation, and UI state |
| `GitHubService` | `Services/GitHubService.cs` | All GitHub access: systems list, `.gitmodules`, repository trees, file downloads |
| `RateLimiter` | `Services/RateLimiter.cs` | Sliding one-hour window that enforces 55/4,900 requests per hour and raises a countdown event |
| `RetryHelper` | `Helpers/RetryHelper.cs` | Exponential backoff for transient errors and helpers to classify failures |
| `SettingsManager` | `Managers/SettingsManager.cs` | Loads/saves encrypted settings and migrates legacy `settings.xml` |
| `UpdateCheckerService` | `Services/UpdateCheckerService.cs` | Queries the latest GitHub release and raises `UpdateAvailable` |
| `BugReportService` | `Services/BugReportService.cs` | Writes `error.log` and forwards actionable errors to the report endpoint |
| `BugReportSink` | `Services/BugReportSink.cs` | Serilog sink that routes Warning/Error events into `BugReportService` |
| `UiLogSink` / `LogConfig` | `Services/UiLogSink.cs`, `Services/LogConfig.cs` | Buffered UI log sink and Serilog configuration |
| `ApplicationStatsService` | `Services/ApplicationStatsService.cs` | Anonymous launch telemetry (app id + version) |
| `ScreenshotService` | `Services/ScreenshotService.cs` | Captures the foreground window for the F8 hotkey |
| `AppInfo` | `Helpers/AppInfo.cs` | Application name, version, and LocalAppData paths |

## Data flow

### Startup

1. `App.OnStartup` initializes Serilog (`LogConfig.Initialize`), registers global exception handlers, and
   fires the anonymous launch telemetry.
2. Command-line arguments are parsed and passed to the main window's `MainViewModel`.
3. `MainViewModel` builds a `GitHubService` from the saved settings and starts `LoadSystemsAsync` plus the
   background update check.

### Systems list

`GitHubService.GetAvailableSystemsAsync` walks two branches (`main`, then `master`):

1. Fetch `.gitmodules` from `raw.githubusercontent.com`; on a 403 it falls back to the GitHub Contents API
   (base64-decoded). A double 403 is translated into a rate-limit `HttpRequestException`.
2. Fetch the recursive repository tree and pair every `commit` entry with the submodule name parsed from
   `.gitmodules` (case-insensitive).
3. On success the list is written to `systems_cache.json`. On rate-limit, authentication, or transient
   failure the cached list is returned instead.

### Prepare

1. `MainViewModel.PrepareDownloadAsync` validates the folders and scans them on a thread-pool thread.
2. Missing covers are the ROM base names that have no matching cover base name (case-insensitive).
3. `GitHubService.GetSystemFilesAsync` retrieves the file list for the selected system
   (`{system}/Named_Boxarts`).
4. Matched entries become `CoverDownloadItem`s with `raw.githubusercontent.com` download URLs.

### Download

1. `MainViewModel.DownloadCoversAsync` filters out items that already exist on disk and items without a URL.
2. For each item, `GitHubService.DownloadFileAsync` downloads the bytes; the view model checks free disk
   space and writes the file using the cover name.
3. Progress, status, and log entries are raised through the dispatcher; cancellation and failures are handled
   per item so a single failure does not abort the batch.

## GitHub interaction details

| Concern | Strategy |
| --- | --- |
| Branch | `main` is tried first, then `master` |
| `.gitmodules` | `raw.githubusercontent.com` first, GitHub Contents API fallback |
| Tree fetch | Recursive tree API; a 500 (repository too large) triggers a non-recursive root tree plus a folder-SHA tree |
| Cover download | `raw.githubusercontent.com` with per-path URL encoding |
| 403 / 429 | Treated as rate limiting: use cache, try the next branch, and log as information |
| 401 | Raises `UnauthorizedAccess`, which prompts for a token in the UI (at most once per service instance) |
| Caching | `systems_cache.json` keeps the system list available offline and under rate limiting |

## Resilience strategy

`RetrySettings` (defaults shown) drives the retry helper:

| Setting | Default | Description |
| --- | --- | --- |
| `MaxRetries` | 3 | Maximum attempts for a single request |
| `BackoffMultiplierSeconds` | 1.5 | Delay is `2^attempt × multiplier` seconds (3s, 6s, 12s) |
| `CircuitBreakerThreshold` | 5 | Consecutive 503 responses before the circuit opens |
| `CircuitBreakerCooldownSeconds` | 30 | Pause applied when the circuit opens |
| `RetryOnForbidden` | `false` | 403 responses normally fall back instead of retrying |

A failure is considered **transient** when it is a 5xx response, a 408/429 response, a socket-level network
error, or an `HttpClient` timeout. Transient failures are retried with exponential backoff; rate limits,
timeouts, and cancellations are logged as informational messages and are not reported as bugs.

## Threading model

- All network and file scanning work is asynchronous (`async`/`await`); folder scans run on the thread pool.
- `MainViewModel` marshals collection and property updates to the WPF dispatcher.
- Cancellation uses a `CancellationTokenSource` created per operation; the UI can cancel preparation or
  download at any time.
- When the token or proxy changes, a new `GitHubService` is created and the old one is queued for disposal
  after in-flight operations complete.

## Diagnostics pipeline

```text
Code ──Serilog──▶ LogConfig ──▶ File sink (logs/log-<date>.txt, 7 days)
                            ├─▶ Debug sink
                            ├─▶ UiLogSink (buffered UI panel)
                            └─▶ BugReportSink (Warning+) ──▶ BugReportService
                                                              ├─▶ error.log (local)
                                                              └─▶ report endpoint
```

Global handlers in `App.xaml.cs` catch dispatcher, AppDomain, and unobserved task exceptions and report them
synchronously before the process exits. Failures inside the reporting pipeline itself are appended to
`critical_error.log` as a last resort.

## Related pages

- [Configuration](configuration.md) — settings, token, proxy, data locations, privacy.
- [Development Guide](development.md) — build, test, and project conventions.
- [Release Process](release-process.md) — how releases and documentation are published.
