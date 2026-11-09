# Development Guide

This page is for developers who want to build, test, or contribute to Retro Game Cover Downloader.

## Prerequisites

| Tool | Notes |
| --- | --- |
| Windows, Linux, or macOS | The application and tests target `net10.0` and use Avalonia |
| [.NET 10 SDK](https://dotnet.microsoft.com/download) | The repository pins SDK `10.0.0` with a `latestMajor` roll-forward policy in `global.json` |
| Git | To clone the repository |
| IDE (optional) | JetBrains Rider, Visual Studio 2022+, or VS Code with the Avalonia extension |

## Get the code

```powershell
git clone https://github.com/purelogiccode/RetroGameCoverDownloader.git
cd CSharp_RetroGameCoverDownloader
```

## Build, run, and test

```powershell
# Restore and build
dotnet restore CSharp_RetroGameCoverDownloader.sln
dotnet build CSharp_RetroGameCoverDownloader.sln -c Release

# Run the application
dotnet run --project RetroGameCoverDownloader

# Run the full unit test suite
dotnet test CSharp_RetroGameCoverDownloader.sln
```

The solution builds with **zero warnings**. The test suite is fully offline and does not require a GitHub
token. CI runs the same build and tests on Windows, Linux, and macOS.

## Cross-platform publish

The release script builds self-contained single-file bundles for all six targets:

```powershell
# All targets
./scripts/package-release.ps1 -Version 2.0.0

# One target at a time
./scripts/package-release.ps1 -Version 2.0.0 -Runtimes linux-arm64 -SkipTests
```

Supported RIDs are `win-x64`, `win-arm64`, `linux-x64`, `linux-arm64`, `osx-x64`, and `osx-arm64`.
Windows bundles are zip files, Linux bundles are tar.gz files, and macOS bundles contain a `.app` directory.
Run the script on the target operating system when you need to preserve Unix executable permissions.

## Project structure

| Project / folder | Contents |
| --- | --- |
| `RetroGameCoverDownloader` | The Avalonia application. Key folders: `Assets`, `Commands`, `Converters`, `Helpers`, `Managers`, `Models`, `Services`, `ViewModels`, `Views` |
| `RetroGameCoverDownloader.Tests` | xUnit tests (also `net10.0`) mirroring the application's folder structure (`Helpers`, `Managers`, `Services`, `ViewModels`, ...) |
| `docs` | This documentation. Sources are plain Markdown plus `toc.yml` and `docfx.json`; `docs/template` customizes the DocFX modern template (dark theme, logo, styling) |
| `scripts` | `package-release.ps1` (release bundles) and `publish-wiki.ps1` (wiki sync) |
| `.github` | `workflows/ci.yml` (build + test), `workflows/codeql.yml` (code scanning), `workflows/release.yml` (approval-gated release), `workflows/docs.yml` (Pages + Wiki), and `dependabot.yml` |

## Testing strategy

- **xUnit** with `Microsoft.NET.Test.Sdk`, `xunit`, and `coverlet.collector` for coverage.
- A `[ModuleInitializer]` replaces `BugReportService` with an in-memory `MockBugReportService`, so unit tests
  never call the bug report API.
- `MainViewModel` exposes `protected virtual` seams (`InvokeOnDispatcher`, `GetFiles`, `WriteAllBytesAsync`,
  `GetAvailableFreeSpace`, ...) that tests override to run without touching the file system, network, or UI.
- `IGitHubService` is fakeable; tests inject an in-memory implementation to simulate repository data,
  rate limits, timeouts, and failures.
- A `CollectingLogSink` captures Serilog events so tests can assert log levels (for example, that transient
  network failures are logged as information rather than errors).

Run a single test class or test with the usual filters:

```powershell
dotnet test --filter "FullyQualifiedName~GitHubServiceTests"
dotnet test --filter "DisplayName~RateLimit"
```

## Static analysis

CodeQL runs in CI for the application and the workflows. You can also run JetBrains InspectCode locally:

```powershell
jb inspectcode CSharp_RetroGameCoverDownloader.sln `
  --output=inspectcode.sarif --format=Sarif --severity=WARNING --swea
```

The solution should report **no warnings or errors**. The SARIF report is for local review only and is
ignored by Git.

## Code conventions

The codebase is C# with nullable reference types enabled, file-scoped namespaces, and structured logging
through Serilog. The following analyzers enforce quality and are part of every build:

- **Meziantou.Analyzer**
- **Roslynator.Analyzers**, **Roslynator.CodeAnalysis.Analyzers**, **Roslynator.Formatting.Analyzers**

Contributions are expected to build with no warnings; a handful of analyzer rules are intentionally disabled
in `.editorconfig` for this codebase.

Guidelines used throughout the project:

- Prefer `async`/`await` end to end; never block the UI thread.
- Marshal UI updates through `MainViewModel`'s dispatcher seams.
- Log with structured message templates (`Log.Information("Loaded {Count} files", count)`).
- Classify failures: transient network problems are expected and logged as information; real defects are
  logged as errors so they reach `error.log` and the bug report endpoint.
- Keep services testable: inject or virtualize external dependencies.

## Working on the documentation

The Markdown files in `docs/` feed two outputs, each with its own lateral menu:

- **GitHub Pages** — built with [DocFX](https://dotnet.github.io/docfx/) using `docs/docfx.json`.
  `docs/toc.yml` defines the top navigation bar, and `docs/guide/toc.yml` defines the left sidebar shown on
  every documentation page.
- **GitHub Wiki** — the same pages are synced by `scripts/publish-wiki.ps1`, which renames `index.md` to
  `Home.md`, flattens folder prefixes, converts `.md` links to wiki-style links, and generates `_Sidebar.md`
  from the navigation file.

When adding a page:

1. Create `docs/guide/<page-name>.md`.
2. Add an entry to `docs/guide/toc.yml` so DocFX adds it to the left sidebar and the wiki `_Sidebar.md`.
3. Link to other pages with relative Markdown links (for example `[User Guide](user-guide.md)`); the wiki
   sync strips folder prefixes and the `.md` extension automatically.

Build the site locally to verify:

```powershell
dotnet tool install --global docfx --version 2.81.0
docfx docs/docfx.json
# Output is written to docs/_site
```

The site uses a dark theme by default. The customization lives in `docs/template`:

- `public/main.js` sets `defaultTheme: 'dark'` and adds the GitHub icon link.
- `public/main.css` overrides the Bootstrap color-mode variables and adds the branding accents.
- `public/logo.png` is the navbar logo.
- `docs/docfx.json` sets `fileMetadata._disableToc` for `index.md` so the landing page keeps the lateral
  menu; without it, DocFX hides the sidebar on pages linked directly from the root `toc.yml`.

## Related pages

- [Architecture](architecture.md) — how the components fit together.
- [Release Process](release-process.md) — CI, packaging, and publishing.
