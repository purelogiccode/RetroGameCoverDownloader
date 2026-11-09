# Security Policy

## Supported versions

| Version | Supported |
| --- | --- |
| 1.5.x | :white_check_mark: |
| < 1.5 | :x: |

Only the latest release receives security fixes. Please update to the newest
[release](https://github.com/purelogiccode/RetroGameCoverDownloader/releases) before reporting an issue.

## Reporting a vulnerability

**Please do not report security vulnerabilities in public issues, discussions, or pull requests.**

Preferred method — GitHub private vulnerability reporting:

<https://github.com/purelogiccode/RetroGameCoverDownloader/security/advisories/new>

If you cannot use GitHub, email **support@purelogiccode.com** with `[SECURITY]` in the subject. Send only a
short first message; if the details are sensitive, wait for a secure channel before sharing them.

### What to include

- Affected version (from the About window or the release ZIP name)
- Windows version and architecture (x64 or arm64)
- A description of the issue and its impact
- Reproduction steps or a proof of concept
- Relevant logs with tokens, proxy passwords, and personal paths removed
- Whether you plan to disclose publicly, and any deadline

## What to expect

| Stage | Target |
| --- | --- |
| Acknowledgement | 3 business days |
| Initial assessment | 10 business days |
| Fix or mitigation | As soon as practical, depending on severity and complexity |
| Coordinated disclosure | 90 days after the report, or earlier by agreement |

We will keep you informed, credit you in the advisory if you want, and coordinate the disclosure date. This
is a community project and does not offer a paid bug bounty.

## Scope

In scope:

- The Windows desktop application and the binaries published in Releases
- Source code on `master`
- GitHub token and proxy credential handling
- Settings encryption and local data storage
- Update-checker behavior
- The bug-report and launch-statistics endpoints as used by the application
- GitHub Actions workflows in `.github/workflows`

Out of scope:

- Vulnerabilities in GitHub, libretro-thumbnails, WPF-UI, Serilog, or other third-party services and packages
  (report those upstream; a heads-up is still welcome if the application is affected)
- Issues that require physical access, a compromised machine, malware, or an attacker who already has
  administrator rights
- Social engineering, spam, and volumetric denial-of-service
- Missing hardening headers or best-practice suggestions with no demonstrated impact
- Reports against versions older than the latest release

## Security design

- **No automatic updates.** The update checker reads the latest GitHub release metadata and opens the
  release page in the browser. It never downloads or executes an update automatically.
- **Token handling.** A GitHub token is optional. The application asks for the `public_repo` scope only,
  sends the token in the `Authorization` header to GitHub hosts (`api.github.com` and
  `raw.githubusercontent.com`), and never writes it to logs or bug reports.
- **Encrypted settings.** `%LocalAppData%\RetroGameCoverDownloader\settings.dat` is encrypted with Windows
  DPAPI (`CurrentUser` scope) using application-specific entropy. Tokens and proxy passwords cannot be
  decrypted by another Windows user or on another machine.
- **HTTPS only.** All GitHub, update-check, telemetry, and bug-report traffic uses HTTPS.
- **Privacy.** Launch telemetry contains only the application id and version. Error reports contain
  environment details and log context, which may include file paths, and are sent only when an actual error
  occurs. See [Configuration → Privacy](docs/guide/configuration.md#privacy).
- **Client identifiers.** The application ships low-privilege identifiers for its own telemetry and
  bug-report endpoints. These are present in every copy of the application and are not user secrets; their
  presence alone is not treated as a vulnerability. Abuse that exposes user data or bypasses authorization
  is in scope.

## Safe harbor

We consider security research conducted in good faith to be authorized. We will not pursue legal action for
research that respects this policy, stays within the scope above, avoids privacy violations and disruption,
and gives us reasonable time to fix the issue before disclosure. If you are unsure whether an activity is
covered, contact us first.

## Related

- [Privacy and data locations](docs/guide/configuration.md#privacy)
- [Architecture](docs/guide/architecture.md)
- [Release process](docs/guide/release-process.md)
