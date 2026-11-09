# AGENTS.md

Guidance for AI coding agents working in this repository. Humans are welcome to use it too. For product and
user documentation, start with [ReadMe.md](ReadMe.md) and [`docs/`](docs).

## Project at a glance

- **What**: Retro Game Cover Downloader — a cross-platform desktop app that scans a ROM folder and downloads
  missing box art from the public [libretro-thumbnails](https://github.com/libretro-thumbnails) repositories.
- **Stack**: C# / .NET 10 (`net10.0`), Avalonia 12 + Fluent dark theme, MVVM, Serilog, xUnit.
- **Platforms**: Windows, Linux, and macOS; x64 and arm64 self-contained builds.
- **Solution**: `CSharp_RetroGameCoverDownloader.sln`
- **Default branch**: `master`
- **Version**: `AssemblyVersion` in `RetroGameCoverDownloader/RetroGameCoverDownloader.csproj` (kept in
  lockstep with the test project)

## Prerequisites

- Windows, Linux, or macOS
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- PowerShell 7 (`pwsh`) or the platform shell equivalents (the commands below use PowerShell)

## Build, run, and test

Run from the repository root:

```powershell
dotnet restore CSharp_RetroGameCoverDownloader.sln
dotnet build CSharp_RetroGameCoverDownloader.sln -c Release --no-restore
dotnet test CSharp_RetroGameCoverDownloader.sln -c Release --no-build
dotnet run --project RetroGameCoverDownloader

# Static analysis (must report zero warnings and errors)
jb inspectcode CSharp_RetroGameCoverDownloader.sln `
  --output=inspectcode.sarif --format=Sarif --severity=WARNING --swea

# Build a single self-contained target (or omit -Runtimes for all six)
./scripts/package-release.ps1 -Version 2.0.0 -Runtimes linux-x64 -SkipTests
```

CI builds and tests on Windows, Linux, and macOS, and publishes `win-x64`, `win-arm64`, `linux-x64`,
`linux-arm64`, `osx-x64`, and `osx-arm64` artifacts.

- The solution must build with **zero warnings**. Fix analyzer findings; do not suppress them without a
  documented reason.
- Tests are fully offline and must stay that way. They never call GitHub, telemetry, or the bug-report
  endpoint.
- Run focused tests with the usual filters:

```powershell
dotnet test --filter "FullyQualifiedName~GitHubServiceTests"
dotnet test --filter "DisplayName~RateLimit"
```

## Repository map

| Path | Contents |
| --- | --- |
| `RetroGameCoverDownloader/` | Avalonia application (`Assets`, `Commands`, `Converters`, `Helpers`, `Managers`, `Models`, `Services`, `ViewModels`, `Views`) |
| `RetroGameCoverDownloader.Tests/` | xUnit suite (`net10.0`) mirroring the app's folder structure |
| `docs/` | DocFX documentation published to GitHub Pages and the Wiki |
| `scripts/` | `package-release.ps1` (release bundles) and `publish-wiki.ps1` (wiki sync) |
| `.github/` | `workflows/` (`ci.yml`, `codeql.yml`, `release.yml`, `docs.yml`) and `dependabot.yml` |

## Architecture essentials

- `MainViewModel` owns scanning, matching, the download loop, cancellation, and UI state. Dialogs use
  code-behind for simplicity.
- `GitHubService` handles all GitHub access (systems list, `.gitmodules`, trees, cover downloads).
  `IGitHubService` exists so tests can inject a fake.
- `RateLimiter` enforces the 55/4,900 requests-per-hour window; `RetryHelper` implements exponential backoff
  and transient-failure classification; `GitHubService` tracks consecutive 503s with a circuit breaker.
- `SettingsManager` persists `settings.dat` with Windows DPAPI (`CurrentUser` scope plus application
  entropy) on Windows and AES-GCM with a user-only `settings.key` on Linux/macOS. It migrates legacy
  `settings.xml` on Windows.
- `AppDialogs` and `PlatformHelper` provide cross-platform message boxes and shell integration. The
  screenshot service renders the Avalonia window instead of using platform-specific screen APIs.
- `BugReportService` / `BugReportSink` write `error.log` and forward actionable errors; transient network
  failures, timeouts, rate limits, and cancellations stay informational and are not reported.
- `ApplicationStatsService` sends anonymous launch telemetry (application id + version only).
- `UpdateCheckerService` reads the latest GitHub release and opens the release page; it never auto-updates.

See [docs/guide/architecture.md](docs/guide/architecture.md) for the full data flow.

## Change recipes

### Add or change a setting

1. Add the property to `Models/AppSettings.cs`.
2. Update `Managers/SettingsManager.cs` only if backward compatibility or migration is required.
3. Wire it through `MainViewModel` and the relevant dialog.
4. Update `docs/guide/configuration.md`.
5. Add or update tests under `RetroGameCoverDownloader.Tests/`.

### Add a service

- Put it in `Services/`; add an interface when tests need a fake.
- Use `async`/`await` end to end, accept a `CancellationToken` where work can be cancelled, and keep
  external dependencies injectable or virtualized.
- Log with Serilog structured templates and classify failures consistently (transient = `Information`,
  real defect = `Error`).
- Add tests in the matching folder under `RetroGameCoverDownloader.Tests/Services/`.

### Update the documentation

1. Create or edit a page in `docs/guide/`.
2. Add new pages to `docs/guide/toc.yml` so DocFX and the wiki sidebar pick them up.
3. Use relative Markdown links such as `[User Guide](user-guide.md)`.
4. Build locally with `dotnet tool install --global docfx --version 2.81.0` and `docfx docs/docfx.json`.
5. Keep the site's dark theme and landing-page sidebar intact: customization lives in `docs/template/`
   (`public/main.js`, `public/main.css`, `public/logo.png`), and `docs/docfx.json` sets
   `fileMetadata._disableToc` for `index.md` because the root TOC links to it directly.

## Code conventions

- Nullable reference types and implicit usings are enabled; use file-scoped namespaces.
- Follow the existing naming patterns: `...Service`, `...Manager`, `...ViewModel`, `...Item`.
- Never block the UI thread and never use `async void` outside Avalonia event handlers. Marshal UI updates
  through the dispatcher seams on `MainViewModel`.
- Use structured logging (`Log.Information("Loaded {Count} files", count)`), not string interpolation, so
  properties stay queryable.
- Keep services testable and deterministic; tests must not depend on the network, the clock, or the real
  file system when a seam already exists.
- Preserve the existing MVVM boundary: business logic in `MainViewModel`/services, presentation in XAML.

## Security rules (non-negotiable)

- Never commit tokens, passwords, API keys, local user paths, or library contents.
- The GitHub token and proxy password are stored only through `SettingsManager` (DPAPI, `CurrentUser` scope,
  application entropy). Do not add plaintext storage or a second secret store.
- Never log `Authorization` headers, tokens, proxy passwords, or full settings files, and never include them
  in bug reports or error messages.
- Keep all network calls on HTTPS. Do not add certificate-validation bypasses or disable TLS checks.
- Do not weaken the privacy behavior. Any new telemetry, remote endpoint, or data collection must be
  documented in `docs/guide/configuration.md` and reviewed.
- GitHub Actions: keep least-privilege `permissions`, avoid `pull_request_target` with untrusted checkouts,
  and never echo secrets.
- Report vulnerabilities privately per [SECURITY.md](SECURITY.md); never open a public issue for one.

## CI and releases

- `.github/workflows/ci.yml` builds and tests every push and pull request to `master`, and uploads test
  results plus coverage.
- `.github/workflows/codeql.yml` runs CodeQL code scanning for C# and GitHub Actions.
- `.github/workflows/docs.yml` builds the DocFX site on PRs and documentation pushes, then deploys to
  GitHub Pages and syncs the wiki on `master`.
- `.github/dependabot.yml` keeps NuGet packages and GitHub Actions up to date every week.
- `.github/workflows/release.yml` is approval-gated and publishes `release_<version>_win-x64.zip` and
  `release_<version>_win-arm64.zip`.
- To cut a release: update `WhatsNew.md`, bump `AssemblyVersion`/`FileVersion` in **both** `.csproj` files,
  push a `release_X.Y.Z` tag, review the bundle checksums, and approve the `release` environment.
  See [docs/guide/release-process.md](docs/guide/release-process.md).

## PR checklist

- [ ] `dotnet build CSharp_RetroGameCoverDownloader.sln -c Release` completes with zero warnings.
- [ ] `dotnet test CSharp_RetroGameCoverDownloader.sln` passes.
- [ ] `jb inspectcode` reports zero warnings and errors.
- [ ] Platform-specific changes compile for Windows, Linux, and macOS (or CI is running).
- [ ] New behavior has tests and existing tests still pass offline.
- [ ] User-facing or configuration changes update `docs/guide/` and `docs/guide/toc.yml`.
- [ ] `WhatsNew.md` is updated for user-visible changes.
- [ ] No secrets, tokens, or personal paths were added.
- [ ] CI is green.

## Related documentation

- [ReadMe.md](ReadMe.md)
- [docs/guide/architecture.md](docs/guide/architecture.md)
- [docs/guide/development.md](docs/guide/development.md)
- [docs/guide/configuration.md](docs/guide/configuration.md)
- [docs/guide/release-process.md](docs/guide/release-process.md)
- [SECURITY.md](SECURITY.md)
