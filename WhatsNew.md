# What's New in 1.5.0

## Smarter GitHub Rate-Limit & Error Handling
- 403 responses from both `raw.githubusercontent.com` and the GitHub Contents API are now recognized as rate limits, so the app correctly falls back to the cached systems list and tries the next branch instead of failing with a generic error.
- 429 (Too Many Requests) responses are treated the same as GitHub rate limits.
- `.gitmodules` fetches now honor your configured retry settings while still skipping retries on 403.
- Repository paths and `.gitmodules` entries are matched case-insensitively for better compatibility.
- Requests you cancel are no longer logged as errors, and timeouts quietly move on to the next branch.

## Fewer Unnecessary Bug Reports
- Transient network failures, timeouts, rate limits, and canceled requests are now logged as informational messages instead of errors, so they are no longer forwarded to the bug-report API.
- Launch telemetry failures are treated as non-critical.
- Context-only log entries (without an exception) are recorded locally instead of being sent as fabricated exceptions.

## More Reliable Commands & Startup
- The Prepare, Download, and Check for Updates commands no longer use `async void`: unexpected exceptions are caught and logged instead of risking an application crash.
- Startup systems loading and update checks are wrapped in safe fire-and-forget helpers.
- The systems list is sorted case-insensitively.

## Locale Fixes
- The proxy port in settings is parsed with the invariant culture, fixing settings that could fail to load on Windows locales that format numbers differently.
- Log timestamps are formatted with the invariant culture.

## New Project Home
- Update checks, download links, and documentation now point to the new repository:
  https://github.com/purelogiccode/RetroGameCoverDownloader

## Under the Hood
- Added **Meziantou.Analyzer** and **Roslynator** analyzers; the solution now builds with zero warnings.
- New CI workflow builds and tests every push and pull request against `master`.
- New approval-gated release workflow publishes `release_1.5.0_win-x64.zip` and `release_1.5.0_win-arm64.zip`, with SHA256 checksums written to the run summary for review before publishing.
- Removed the token-gated live GitHub integration tests; the full unit test suite now runs everywhere without a `GITHUB_TOKEN`.
- Version bumped to 1.5.0.

**Full Changelog**: https://github.com/purelogiccode/RetroGameCoverDownloader/compare/release_1.4.0...release_1.5.0
