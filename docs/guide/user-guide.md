# User Guide

This page explains every part of the application and the complete cover download workflow.

## The main window

| Area | Description |
| --- | --- |
| **Title bar** | Fluent title bar with the application name and standard window controls |
| **Menu bar** | **File** and **Help** menus — see [Menu reference](#menu-reference) |
| **Update banner** | Appears when a newer release is available; links to the release page and can be dismissed |
| **ROM Folder** | The folder containing your ROM files (read-only field, set with the button) |
| **Cover Folder** | The folder where downloaded covers are saved (read-only field, set with the button) |
| **Select System** | Drop-down list of systems discovered from libretro-thumbnails |
| **Prepare for Download** | Scans both folders and matches missing covers against GitHub |
| **Download Covers** | Downloads every matched cover |
| **Cancel** | Stops the current preparation or download operation |
| **Status line** | Current operation, progress summary, and the rate-limit countdown |
| **Progress bar** | Completed items versus total items for the running operation |
| **Log** | Detailed, timestamped log of everything the application does |

## Download workflow

### 1. Choose your folders

- Click **Set ROM Folder** and select the folder that contains your ROM files.
- Click **Set Cover Folder** and select the folder where cover images should be written.
  Keeping covers in a separate folder (for example `C:\Covers\SNES`) is recommended; the application never
  modifies your ROM files.

### 2. Select a system

The **Select System** list is populated from the libretro-thumbnails repositories. The list is cached under
`%LocalAppData%\RetroGameCoverDownloader\systems_cache.json`, so it also appears when GitHub is temporarily
unreachable. Use the system that matches your ROM set — for example *Nintendo - Super Nintendo Entertainment
System* for SNES ROMs.

### 3. Prepare for Download

**Prepare for Download** performs these steps:

1. Validates that both folders exist.
2. Scans the ROM folder for files with the [configured extensions](configuration.md#file-extensions).
3. Scans the cover folder for existing covers.
4. Computes the ROMs that do not have a matching cover yet.
5. Downloads the file list for the selected system from GitHub.
6. Matches every missing ROM against the available cover files.

**Matching rules**

- A cover matches a ROM when the file name **without its extension** is equal, ignoring case.
  `Super Mario Bros.nes` matches `Super Mario Bros.png`.
- Only files inside the system's `Named_Boxarts` folder are considered.
- Files starting with a dot (for example `.gitkeep`) are ignored.

When the operation finishes, the log reports how many covers are available for download. The **Download
Covers** button becomes enabled as soon as at least one item is matched.

### 4. Download Covers

The download stage:

- Skips items whose destination file already exists.
- Checks free disk space before each write and aborts the batch if less than 10 MB is available.
- Writes each cover with the same base name as the ROM file (for example `Super Mario Bros.png`).
- Reports each attempt, retry, and failure in the log and updates the progress bar.

If a download fails, the item is skipped and the batch continues with the next cover. Re-running
**Prepare for Download** and **Download Covers** retries only what is still missing.

### 5. Cancel

**Cancel** stops the running operation safely:

- Preparation stops as soon as the current scan/request finishes.
- Downloading stops after the current file; completed covers are kept.

## Rate limits

GitHub limits how many requests an application may make per hour. Retro Game Cover Downloader applies its
own conservative throttle:

| Mode | Limit enforced by the application | Typical GitHub limit |
| --- | --- | --- |
| Without a token | 55 requests/hour | 60 requests/hour per IP |
| With a token | 4,900 requests/hour | 5,000 requests/hour per token |

When the limit is reached, the status line shows a countdown (**Rate limit reached. Resuming in N
seconds...**) and the application waits before continuing. Adding a token is strongly recommended — see
[Configuration](configuration.md#github-token).

## Authentication errors (401)

If GitHub reports that your token is missing, invalid, or expired, the application:

1. Shows a message box explaining the problem.
2. Opens the **GitHub Token** dialog so you can paste a new token.
3. Saves the token (encrypted) and automatically reloads the systems list with the new credentials.

Dismiss the dialog to continue with anonymous access.

## Update notifications

The application checks for a newer release in the background at startup and whenever you use
**Help → Check for Updates**. When a newer version is found, an update banner appears above the main content
with a **Download now** link and a **Download** button, both of which open the release page in your browser.
Use **✕** to dismiss the banner for the session.

## Menu reference

| Menu | Item | Description |
| --- | --- | --- |
| File | Proxy Settings | Enable/disable an HTTP proxy and configure host, port, and credentials |
| File | GitHub Token | Enter or replace your personal access token |
| File | File Extensions | Choose which file extensions are treated as ROM files |
| File | Open AppData Path | Opens `%LocalAppData%\RetroGameCoverDownloader` in File Explorer |
| File | Exit | Closes the application cleanly |
| Help | Check for Updates | Queries GitHub for the newest release |
| Help | About | Shows version, author, and project links |

## Screenshots (F8)

Press **F8** at any time to capture the foreground window. Screenshots are saved to
`%LocalAppData%\RetroGameCoverDownloader\Screenshots` and each capture is logged. This is useful when
reporting a problem.

## Log panel

The log panel shows the same structured log that the application writes to disk. It is trimmed automatically
to keep memory usage bounded. Use **File → Open AppData Path** to open the log folder directly when you need
to share a log with a developer.

## Next steps

- [Configuration](configuration.md) — token, proxy, extensions, and data locations.
- [Troubleshooting](troubleshooting.md) — what to do when something does not work.
