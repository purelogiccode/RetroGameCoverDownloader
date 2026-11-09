# What's New in 2.0.2

## Maintenance Release
- Re-validated the solution with JetBrains InspectCode: **zero warnings and errors**.
- The full unit test suite passes on Windows, Linux, and macOS.
- No functional changes; this release republishes the 2.0.1 code with refreshed build metadata and bundles.
- Includes the 2.0.1 About window icon and layout fix.

## About Window Fix (2.0.1)
- The About window now uses the same application icon as the main window.
- The About layout is tighter and vertically centered, with a smaller logo and less empty space.

## 2.0.0 Highlights

### Cross-Platform UI
- The entire desktop UI has been migrated from WPF to **Avalonia 12** and now runs on
  **Windows, Linux, and macOS**.
- A modern Fluent dark theme, custom card layout, app header, and refreshed dialogs replace the old WPF chrome.
- Native folder pickers and message dialogs work on every platform.
- **F8** captures the application window while it is focused and saves a PNG to the AppData screenshots folder.
- **Open AppData Folder**, update links, and the About link use platform-aware shell integration.

### Six Self-Contained Builds
- Release bundles are now produced for:
  - `win-x64` and `win-arm64` (zip)
  - `linux-x64` and `linux-arm64` (tar.gz)
  - `osx-x64` and `osx-arm64` (tar.gz containing a `.app` bundle)
- Every bundle is self-contained and single-file, so no separate .NET runtime install is required.
- CI builds and tests on Windows, Linux, and macOS, and publishes all six RID artifacts.
- The release workflow builds each bundle on its native operating system so Unix executable permissions are
  preserved.

### Cross-Platform Settings Security
- Windows keeps using **Windows DPAPI** (`CurrentUser` scope) for `settings.dat`.
- Linux and macOS use **AES-GCM** with a randomly generated 256-bit key stored next to the settings file with
  user-only permissions (0600).
- The settings format detects the protection type automatically, and legacy DPAPI `settings.xml` migration
  remains available on Windows.

### Reliability & CI
- GitHub `429` rate limits are now handled consistently with `403` rate limits, including the repository tree
  API used to list covers.
- The Check for Updates menu uses the view-model command instead of duplicate window code-behind.
- The solution builds with zero warnings and reports no JetBrains InspectCode warnings or errors.
- CI now collects code coverage and validates documentation builds on pull requests.
- CodeQL scans the C# application and GitHub Actions workflows; Dependabot keeps NuGet packages and actions
  current.
- The security policy supports private vulnerability reporting through GitHub Security Advisories.

### Previous 1.5.0 Highlights
- Smarter GitHub rate-limit handling, cached systems fallback, and case-insensitive repository matching.
- Transient network failures, timeouts, rate limits, and canceled requests are no longer forwarded to the
  bug-report API.
- Safer async command handling, invariant-culture parsing, and improved startup diagnostics.
- Multi-page documentation on GitHub Pages and the Wiki, with a dark theme and lateral menu.

**Full Changelog**: https://github.com/purelogiccode/RetroGameCoverDownloader/compare/release_2.0.1...release_2.0.2
