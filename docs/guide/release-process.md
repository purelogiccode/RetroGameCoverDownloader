# Release Process

This page describes how versions are managed and how builds, releases, and documentation are published by
the project's GitHub Actions pipelines.

## Versioning

The application uses three-part versions such as `2.0.0`:

| Location | Purpose |
| --- | --- |
| `RetroGameCoverDownloader/RetroGameCoverDownloader.csproj` → `AssemblyVersion`, `FileVersion` | Application version, checked by the release pipeline |
| `RetroGameCoverDownloader.Tests/RetroGameCoverDownloader.Tests.csproj` → `AssemblyVersion`, `FileVersion` | Kept in lockstep with the application |
| `WhatsNew.md` | User-facing release notes for the upcoming version |
| Git tag `release_<version>` | Triggers the release pipeline and names the GitHub release |

## Continuous integration

`.github/workflows/ci.yml` runs on every push to `master`, every pull request, and on demand:

1. Checkout and .NET 10 SDK setup (with NuGet cache) on Windows, Linux, and macOS.
2. `dotnet restore` and `dotnet build -c Release`.
3. `dotnet test -c Release` — the full unit test suite. Test results and code coverage are uploaded as
   an artifact.
4. A publish job builds self-contained bundles for all six RIDs and uploads them as workflow artifacts.

`.github/workflows/codeql.yml` runs CodeQL analysis for the C# application and the GitHub Actions
workflows on every push and pull request, plus a weekly scheduled scan. `.github/dependabot.yml` checks
NuGet packages and actions every week; Dependabot alerts and security updates are enabled in the
repository settings.

## Release pipeline

`.github/workflows/release.yml` is triggered by pushing a tag such as `release_2.0.0`, or manually with a
version input. It runs four jobs:

| Job | What it does |
| --- | --- |
| **version** | Resolves the version from the tag or input and validates the `X.Y.Z` format |
| **verify** | Fails if `AssemblyVersion` does not match the requested version, then builds and tests the solution |
| **bundles** | Runs `scripts/package-release.ps1 -SkipTests` in a six-target matrix to build self-contained bundles for `win-x64`, `win-arm64`, `linux-x64`, `linux-arm64`, `osx-x64`, and `osx-arm64`; writes each SHA256 checksum to the run summary and uploads it as an artifact |
| **publish** | Waits for approval in the protected `release` environment, downloads all six bundles, verifies they are present, and creates or updates the GitHub release with `WhatsNew.md` as the release notes |

### Bundle contents

Each bundle contains the self-contained single-file application and the documentation:

- Windows: `RetroGameCoverDownloader.exe`, `ReadMe.md`, `LICENSE.txt`, `WhatsNew.md` (`.zip`)
- Linux: `RetroGameCoverDownloader`, `ReadMe.md`, `LICENSE.txt`, `WhatsNew.md` (`.tar.gz`)
- macOS: `RetroGameCoverDownloader.app` with the documentation in `Contents/Resources` (`.tar.gz`)

No separate .NET runtime is required.

### Packaging locally

The same script used by CI can be run from a developer machine:

```powershell
# Run tests and build all six bundles
.\scripts\package-release.ps1 -Version 2.0.0

# Skip tests (CI runs them in a separate job) and stage elsewhere
.\scripts\package-release.ps1 -Version 2.0.0 -SkipTests -StagingDirectory C:\Temp\rgcd-staging

# Build one target
.\scripts\package-release.ps1 -Version 2.0.0 -Runtimes linux-arm64 -SkipTests
```

Bundles are written to `RetroGameCoverDownloader\bin\Release` by default, and the script prints the SHA256
hash of each archive.

### Required repository configuration

| Setting | Where | Purpose |
| --- | --- | --- |
| `release` environment with required reviewers | Settings → Environments | Approval gate before the release is published |
| Workflow permissions | Repository default settings | The workflow uses `contents: write` only in the publish job |
| Pages source: **GitHub Actions** | Settings → Pages | Allows the docs workflow to deploy the site |
| Wikis enabled + an initial page | Settings → Features → Wikis | The wiki repository must exist before it can be cloned by CI |
| `WIKI_TOKEN` secret | Settings → Secrets and variables → Actions | Classic PAT with `repo` scope, used to push documentation to the wiki; without it the wiki job is skipped |

## Documentation pipeline

`.github/workflows/docs.yml` runs when documentation changes are pushed to `master`, on pull requests
that touch documentation (build validation only), and on demand:

1. Builds the DocFX site from `docs/docfx.json`.
2. Deploys the generated site to **GitHub Pages** at
   <https://purelogiccode.github.io/RetroGameCoverDownloader/> using the official Pages actions
   (push and manual runs only).
3. Syncs the Markdown sources to the **GitHub Wiki** with `scripts/publish-wiki.ps1`
   (push and manual runs only) and verifies that `Home.md` and `_Sidebar.md` exist:
   - `index.md` is published as `Home.md`.
   - Pages are flattened into the wiki root and `.md` links are converted to wiki-style links.
   - `_Sidebar.md` is generated from `docs/guide/toc.yml`, giving every wiki page a lateral menu.
   - Pages are added or updated; unrelated wiki pages are left untouched.

## Cutting a release

1. Update `WhatsNew.md` with the user-facing changes.
2. Bump `AssemblyVersion` and `FileVersion` in both `.csproj` files to the new version.
3. Commit and push to `master`; make sure CI is green.
4. Push the tag:

   ```powershell
   git tag release_2.0.0
   git push origin release_2.0.0
   ```

   Alternatively, run the **Release** workflow manually and provide the version.
5. Review the **bundles** job summary (sizes and SHA256 hashes) and approve the `release` environment to
   publish.
6. Verify the release page and the updated documentation links.

## Maintenance notes

- Keep `WhatsNew.md` accurate for the next version; it becomes the public release description.
- The release pipeline accepts only three-part versions; pre-release suffixes are not supported.
- The `verify` job fails early when the tag and the project version differ, preventing mislabeled bundles.
- Documentation is versionless by design — it always describes the current `master` state.
