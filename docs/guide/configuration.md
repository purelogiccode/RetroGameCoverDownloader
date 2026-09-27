# Configuration

Retro Game Cover Downloader is ready to use out of the box. This page covers the optional settings that
unlock higher download limits, support restricted networks, and control how ROM files are detected.

## Settings storage

All settings are stored in a single encrypted file:

| Item | Location |
| --- | --- |
| Settings | `%LocalAppData%\RetroGameCoverDownloader\settings.dat` |
| Systems cache | `%LocalAppData%\RetroGameCoverDownloader\systems_cache.json` |
| Logs | `%LocalAppData%\RetroGameCoverDownloader\logs\log-<date>.txt` |
| Error log | `%LocalAppData%\RetroGameCoverDownloader\error.log` |
| Critical log | `%LocalAppData%\RetroGameCoverDownloader\critical_error.log` |
| Screenshots | `%LocalAppData%\RetroGameCoverDownloader\Screenshots` |

The settings file is encrypted with **Windows DPAPI** (CurrentUser scope): the encryption key is derived from
your Windows account and never leaves the machine. The file cannot be decrypted by another Windows user or on
another PC — keep this in mind when migrating your profile.

> **Tip**
> Use **File → Open AppData Path** in the application to open this folder directly.

If a legacy `settings.xml` (versions 1.4.0 and earlier) is found next to the executable, it is migrated to
the new encrypted format automatically on first load.

## GitHub token

A token is optional, but strongly recommended:

| Mode | Requests per hour |
| --- | --- |
| Anonymous | 55 (GitHub allows 60 per IP) |
| With a token | 4,900 (GitHub allows 5,000 per token) |

To create a token:

1. Open **File → GitHub Token** in the application (the dialog also appears automatically on first launch).
2. In a browser, go to <https://github.com/settings/tokens/new>.
3. Give the token a name, set an expiration, and select the **`public_repo`** scope.
   The application only reads public libretro-thumbnails repositories, so no other scopes are needed.
4. Generate the token, copy it, and paste it into the dialog.

The token is stored inside the encrypted `settings.dat` file. If GitHub later reports **401 Unauthorized**
(expired or revoked token), the application prompts for a replacement and reloads the system list
automatically.

## Proxy settings

Use **File → Proxy Settings** when your network requires an HTTP proxy:

| Field | Notes |
| --- | --- |
| Enabled | Turns proxy usage on or off |
| Host | Host name or URL; `http://` and `https://` prefixes are accepted and stripped |
| Port | 1–65535 |
| Username / Password | Optional; only sent when both are provided |

Proxy changes are applied immediately: the application creates a new GitHub service, keeps in-flight
operations safe, and logs the change. Proxy credentials are part of the encrypted settings file.

## File extensions

**File → File Extensions** controls which files are treated as ROMs when scanning. Defaults:

```text
.nes .sfc .smc .md .gen .gba .gb .gbc .n64 .z64 .v64 .iso .cue .bin .img .ccd .chd
.zip .7z .rar .rom .smd .gg .pce .lnx .ws .wsc .a78 .a26 .int .col
```

- The leading dot is added automatically if you omit it.
- Matching is case-insensitive.
- **Reset** restores the default list.
- Clearing the list disables extension filtering entirely, so every file in the ROM folder is considered.

## Command-line arguments

| Argument | Description |
| --- | --- |
| `--rom <path>` or `/rom <path>` | Pre-fills the ROM folder |
| `--cover <path>` or `/cover <path>` | Pre-fills the cover folder |
| Two positional paths | First path is the ROM folder, second is the cover folder |

See [Getting Started](getting-started.md#command-line-usage) for examples.

## Privacy

Retro Game Cover Downloader collects the minimum amount of data needed to operate and improve the
application:

- **Launch telemetry** — an anonymous request containing only the application id and version is sent at
  startup. It contains no personal data, file paths, or game names. If the endpoint is unreachable or the
  request times out, the failure is logged locally as information and ignored.
- **Error reporting** — warnings and errors are written to `error.log` with environment details (operating
  system, application version, paths) and sent to the developer's bug report endpoint to help fix problems.
  Since version 1.5.0, transient network failures, timeouts, rate limits, and canceled operations are logged
  as informational messages only and are **not** forwarded to the bug report endpoint.
- **No tracking of your library** — ROM names, cover names, and folder paths stay on your machine. Only error
  diagnostics, which can include such details inside log messages, are reported when an actual error occurs.

Deleting the `%LocalAppData%\RetroGameCoverDownloader` folder removes all local data (settings, cache, logs,
and screenshots). You can also remove an unwanted token at any time from **File → GitHub Token**.

## Next steps

- [User Guide](user-guide.md) — the complete download workflow.
- [Troubleshooting](troubleshooting.md) — common problems and solutions.
