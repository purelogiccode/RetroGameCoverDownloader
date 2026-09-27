# Troubleshooting

This page lists the most common problems and how to resolve them. Most issues leave a trace in the log;
open **File → Open AppData Path** to find the log files described in [Configuration](configuration.md).

## The system list is empty

**Symptoms:** the **Select System** drop-down stays empty and the log reports a rate limit or a transient
network error.

**Causes and solutions:**

1. **GitHub rate limit** — without a token GitHub allows only 60 requests/hour per IP. Wait for the limit to
   reset, or add a [GitHub token](configuration.md#github-token) to raise it to 5,000/hour.
2. **Network or proxy problems** — verify your internet connection and, if you use a proxy, check
   [Proxy settings](configuration.md#proxy-settings).
3. **No network at all** — the application falls back to the cached list at
   `%LocalAppData%\RetroGameCoverDownloader\systems_cache.json`. If the cache is missing or empty the list
   cannot be populated until GitHub becomes reachable.

After fixing the cause, restart the application or use **File → GitHub Token** to retry with new credentials.

## The application asks for a token repeatedly

A **401 Unauthorized** prompt appears when GitHub rejects the token — it may be expired, revoked, or missing
the required scope.

1. Create a new token at <https://github.com/settings/tokens/new> with the **`public_repo`** scope.
2. Paste it into the **GitHub Token** dialog. The application saves it (encrypted) and reloads the systems
   list automatically.

Dismissing the dialog keeps you on anonymous access; the prompt appears at most once per session for the
current service instance.

## "Rate limit reached. Resuming in N seconds..."

This is expected behavior. GitHub's hourly limits apply to every client:

| Mode | Limit |
| --- | --- |
| Anonymous | 55 requests/hour (application limit; GitHub allows 60) |
| Token | 4,900 requests/hour (GitHub allows 5,000) |

The countdown in the status bar tells you when the application will continue. To avoid waiting, add a token.
The application deliberately waits instead of failing so large batches finish reliably.

## Covers are not found during Prepare

If the log reports that no covers matched:

1. **Check the file names** — matching is by file name without extension, ignoring case.
   `Super Mario Bros.nes` matches `Super Mario Bros.png`, but `SuperMarioBros.nes` does not.
2. **Check the extensions** — files with extensions that are not in the
   [extension list](configuration.md#file-extensions) are skipped entirely.
3. **Check the system** — the selected system must contain the game. Some games exist for several systems;
   pick the entry that matches your ROM set.
4. **Check the folder** — make sure the ROM folder really contains the files (subfolders are not scanned).

## Downloads fail

The log reports each failure with a reason. Common causes:

| Log message | Meaning | Solution |
| --- | --- | --- |
| `Failed to download ...` | Network hiccup or the file is unavailable | Re-run **Prepare for Download** and **Download Covers**; only missing files are retried |
| `Server busy (503 ...). Retrying in ...` | GitHub is temporarily unavailable | Wait; the circuit breaker pauses automatically after repeated 503s |
| `Low disk space detected` | Less than 10 MB free on the cover drive | Free space and start the download again |
| `File was not created at ...` | The cover folder is not writable | Choose a writable folder or fix the folder permissions |

## Folders do not exist

The **Prepare** operation validates both folders before scanning. If a folder was moved, deleted, or renamed,
re-select it with **Set ROM Folder** / **Set Cover Folder**.

## Proxy does not work

1. Verify the host and port with your network administrator.
2. Remove `http://` or `https://` from the host field — the application strips them automatically, but a
   typo in the remainder prevents connections.
3. If the proxy requires authentication, supply **both** username and password.
4. Check the log after changing settings; the application logs the active proxy status when it applies new
   settings.

## Updates are not detected

- **Help → Check for Updates** queries the latest GitHub release. If the request fails (offline, rate limit),
  a message is written to the log and the check is retried on the next launch.
- The update check runs against `purelogiccode/RetroGameCoverDownloader`. If you built the application from a
  fork, the version comparison still works, but releases from the original repository are used.

## Settings are not remembered or the file cannot be read

Settings are encrypted with Windows DPAPI scoped to your Windows account. Moving `settings.dat` to another
user account or another computer makes it unreadable — the application logs a warning and starts with
defaults. Re-enter the token and proxy settings in that case.

## F8 screenshot does not work

The F8 hotkey is registered globally when the main window opens. If another application already owns F8, the
log reports **"Failed to register F8 global hotkey"** and the hotkey is unavailable until you close the
conflicting application and restart. Normal screenshots using Windows tools still work as usual.

## Reporting a problem

When reporting an issue, include:

1. The application version (**Help → About**).
2. The relevant excerpt from the log (or the whole `logs` folder).
3. `error.log` if the application reported an error.
4. Steps to reproduce the problem.

Attach the files to a new issue at
<https://github.com/purelogiccode/RetroGameCoverDownloader/issues>.

> **Tip**
> The application writes detailed diagnostics to `error.log` and, for critical failures, to
> `critical_error.log`. Review these files before sharing — they can contain folder paths and game names.
