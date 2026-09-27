# Getting Started

This page walks you through installing Retro Game Cover Downloader and downloading your first covers.

## Requirements

| Requirement | Details |
| --- | --- |
| Operating system | Windows 10 or later (x64 or arm64) |
| Runtime | .NET 10 Desktop Runtime — Windows prompts you to download it automatically if it is missing |
| Network | Internet access to GitHub. A personal access token is recommended but optional |
| Disk space | A few megabytes for the application plus space for the downloaded cover images |

## Install

1. Open the [Releases page](https://github.com/purelogiccode/RetroGameCoverDownloader/releases).
2. Download the bundle that matches your PC:
   - `release_<version>_win-x64.zip` for the vast majority of Windows PCs.
   - `release_<version>_win-arm64.zip` for Windows on ARM devices.
3. Extract the zip to a folder of your choice, for example `C:\Tools\RetroGameCoverDownloader`.

Each bundle contains:

| File | Purpose |
| --- | --- |
| `RetroGameCoverDownloader.exe` | The application (framework-dependent single-file executable) |
| `ReadMe.md` | Quick overview and links |
| `LICENSE.txt` | The GPL-3.0 license text |
| `WhatsNew.md` | Release highlights for this version |

> **Note**
> The application is framework-dependent. If the .NET 10 Desktop Runtime is not installed, Windows offers to
> download it the first time you launch the application.

## First run

1. Double-click `RetroGameCoverDownloader.exe`.
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
# Positional arguments: ROM folder first, then cover folder
RetroGameCoverDownloader.exe "C:\ROMs\SNES" "C:\Covers\SNES"

# Named arguments (both / and -- prefixes are accepted)
RetroGameCoverDownloader.exe --rom "C:\ROMs" --cover "C:\Covers"
RetroGameCoverDownloader.exe /rom "C:\ROMs" /cover "C:\Covers"
```

Paths passed on the command line are pre-filled in the main window; everything else behaves exactly like a
normal launch.

## Next steps

- Read the [User Guide](user-guide.md) for a full tour of the interface.
- Set up a token and proxy in [Configuration](configuration.md).
- Review [Troubleshooting](troubleshooting.md) if a download does not start.
