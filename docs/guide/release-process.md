# Release Process

This page describes how versions are managed and how builds, releases, and documentation are published by
the project's GitHub Actions pipelines.

## Versioning

The application uses three-part versions such as `1.5.0`:

| Location | Purpose |
| --- | --- |
| `RetroGameCoverDownloader/RetroGameCoverDownloader.csproj` → `AssemblyVersion`, `FileVersion` | Application version, checked by the release pipeline |
| `RetroGameCoverDownloader.Tests/RetroGameCoverDownloader.Tests.csproj` → `AssemblyVersion`, `FileVersion` | Kept in lockstep with the application |
| `WhatsNew.md` | User-facing release notes for the upcoming version |
| Git tag `release_<version>` | Triggers the release pipeline and names the GitHub release |

## Continuous integration

`.github/workflows/ci.yml` runs on every push to `master`, every pull request, and on demand:

1. Checkout and .NET 10 SDK setup (with NuGet cache).
2. `dotnet restore` and `dotnet build -c Release`.
3. `dotnet test -c Release` — the full unit test suite. Test results are uploaded as an artifact.

## Release pipeline

`.github/workflows/release.yml` is triggered by pushing a tag such as `release_1.5.0`, or manually with a
version input. It runs four jobs:

| Job | What it does |
| --- | --- |
| **version** | Resolves the version from the tag or input and validates the `X.Y.Z` format |
| **verify** | Fails if `AssemblyVersion` does not match the requested version, then builds and tests the solution |
| **bundles** | Runs `scripts/package-release.ps1 -SkipTests` to publish the framework-dependent single-file executable for `win-x64` and `win-arm64`, zips each with the documentation, writes SHA256 checksums to the run summary, and uploads the `release-bundles` artifact |
| **publish** | Waits for approval in the protected `release` environment, downloads the reviewed bundles, verifies both zips are present, and creates or updates the GitHub release with `WhatsNew.md` as the release notes |

### Bundle contents

Each `release_<version>_win-<arch>.zip` contains:

- `RetroGameCoverDownloader.exe` (framework-dependent single-file publish)
- `ReadMe.md`
- `LICENSE.txt`
- `WhatsNew.md`

### Packaging locally

The same script used by CI can be run from a developer machine:

```powershell
# Run tests, publish both architectures, and write the zips
.\scripts\package-release.ps1 -Version 1.5.0

# Skip tests (CI runs them in a separate job) and stage elsewhere
.\scripts\package-release.ps1 -Version 1.5.0 -SkipTests -StagingDirectory C:\Temp\rgcd-staging
```

Bundles are written to `RetroGameCoverDownloader\bin\Release` by default, and the script prints the SHA256
hash of each zip.

### Required repository configuration

| Setting | Where | Purpose |
| --- | --- | --- |
| `release` environment with required reviewers | Settings → Environments | Approval gate before the release is published |
| Workflow permissions | Repository default settings | The workflow uses `contents: write` only in the publish job |
| Pages source: **GitHub Actions** | Settings → Pages | Allows the docs workflow to deploy the site |
| Wikis enabled + an initial page | Settings → Features → Wikis | The wiki repository must exist before it can be cloned by CI |
| `WIKI_TOKEN` secret | Settings → Secrets and variables → Actions | Classic PAT with `repo` scope, used to push documentation to the wiki; without it the wiki job is skipped |

## Documentation pipeline

`.github/workflows/docs.yml` runs when documentation changes are pushed to `master` (and on demand):

1. Builds the DocFX site from `docs/docfx.json`.
2. Deploys the generated site to **GitHub Pages** at
   <https://purelogiccode.github.io/RetroGameCoverDownloader/> using the official Pages actions.
3. Syncs the Markdown sources to the **GitHub Wiki** with `scripts/publish-wiki.ps1`:
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
   git tag release_1.5.0
   git push origin release_1.5.0
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
