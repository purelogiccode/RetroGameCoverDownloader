# Getting Started

This page walks you through installing Retro Game Cover Downloader and downloading your first covers.

## Requirements

| Requirement | Details |
| --- | --- |
| Operating system | Windows 10 or later, Linux (x64 or arm64), or macOS 11 or later (Intel or Apple silicon) |
| Runtime | None — release bundles are self-contained and include .NET 10 |
| Network | Internet access to GitHub. A personal access token is recommended but optional |
| Disk space | A few megabytes for the application plus space for the downloaded cover images |

## Install

1. Open the [Releases page](https://github.com/purelogiccode/RetroGameCoverDownloader/releases).
2. Download the bundle that matches your OS and CPU:
   - `release_<version>_win-x64.zip` or `release_<version>_win-arm64.zip`
   - `release_<version>_linux-x64.tar.gz` or `release_<version>_linux-arm64.tar.gz`
   - `release_<version>_osx-x64.tar.gz` or `release_<version>_osx-arm64.tar.gz`
3. Extract the archive to a folder of your choice.

Each Windows bundle contains:

| File | Purpose |
| --- | --- |
| `RetroGameCoverDownloader.exe` | The application (self-contained single-file executable) |
| `ReadMe.md` | Quick overview and links |
| `LICENSE.txt` | The GPL-3.0 license text |
| `WhatsNew.md` | Release highlights for this version |

Linux bundles contain the `RetroGameCoverDownloader` executable and the same documentation. macOS bundles
contain `RetroGameCoverDownloader.app` with the documentation in `Contents/Resources`.

> **Note**
> Release bundles are self-contained. No separate .NET runtime is required. On Linux, use a desktop session
> with X11 or XWayland and the standard font libraries. On macOS, the app is unsigned; right-click the app
> and choose **Open** the first time if Gatekeeper warns you.

## First run

- **Windows:** double-click `RetroGameCoverDownloader.exe`.
- **Linux:** run `./RetroGameCoverDownloader` from a terminal (or mark it executable in your file manager).
- **macOS:** open `RetroGameCoverDownloader.app`.
2. The application opens with the main window and starts loading the available systems in the background.
   The log panel reports progress; the system list is cached so later starts are instant.
3. On first launch a **GitHub token** dialog appears. You can skip it, but a token raises the download limit
   from 55 to 4,900 requests per hour. See [Configuration](configuration.md) for details.

## Quick start

1. **Set ROM Folder** — browse to the folder that contains your ROM files.
2. **Set Cover Folder** — browse to the folder where cover images should be saved (create a dedicated folder
   if you do not have one yet).
3. **Select System** — pick the system from the drop-down list (NES, SNES, Genesis, ...).
4. Click **Prepare for Download** — the application scans both folders and matches ROM file names against the
   covers available in the libretro-thumbnails repository for that system.
5. Click **Download Covers** — every matched cover is downloaded into the cover folder. The progress bar and
   the log panel keep you informed.

You can press **Cancel** at any time during preparation or downloading. Completed downloads are kept; the
operation stops after the current file.

## Command-line usage

The application accepts ROM and cover folders as arguments, which makes it convenient to launch from
scripts or shortcuts:

```powershell
# Windows
RetroGameCoverDownloader.exe "C:\ROMs\SNES" "C:\Covers\SNES"
RetroGameCoverDownloader.exe --rom "C:\ROMs" --cover "C:\Covers"

# Linux / macOS
./RetroGameCoverDownloader "/home/me/ROMs/SNES" "/home/me/Covers/SNES"
./RetroGameCoverDownloader --rom "/home/me/ROMs" --cover "/home/me/Covers"
```

Both `/rom` and `--rom` prefixes are accepted, along with `/cover` and `--cover`.

Paths passed on the command line are pre-filled in the main window; everything else behaves exactly like a
normal launch.

## Next steps

- Read the [User Guide](user-guide.md) for a full tour of the interface.
- Set up a token and proxy in [Configuration](configuration.md).
- Review [Troubleshooting](troubleshooting.md) if a download does not start.
