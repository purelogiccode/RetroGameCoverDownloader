# Frequently Asked Questions

## General

### What does Retro Game Cover Downloader do?

It scans your ROM folders, compares them with the cover art available in the community
[libretro-thumbnails](https://github.com/libretro-thumbnails) repositories, and downloads the missing covers
into a folder you choose.

### Which systems are supported?

All systems published under the libretro-thumbnails organization. The list is discovered automatically from
GitHub and cached locally, so new systems appear without an application update. Examples: NES, SNES, Genesis,
Game Boy, Game Boy Advance, Nintendo 64, PlayStation, arcade boards, and many more.

### Is it free?

Yes. The application is open source under the **GPL-3.0 license**. Cover art comes from the community
libretro-thumbnails project and remains the property of the respective copyright holders; the application
only downloads these publicly available files.

### Does it work on macOS or Linux?

No. It is a Windows desktop application built with WPF and the .NET 10 Desktop Runtime.

## Usage

### Do I need a GitHub token?

No, but it is strongly recommended. Anonymous access is limited to about 55 requests per hour by the
application (GitHub allows 60). A token raises this to 4,900 requests per hour (GitHub allows 5,000), which
matters for large collections. See [Configuration](configuration.md#github-token).

### Does it rename or modify my ROM files?

Never. The application only reads your ROM file names to match covers. Cover images are written to a separate
folder with the same base name as the matching ROM.

### Can I download covers for several systems at once?

The application works one system at a time. Select a system, prepare, download, then switch to the next
system. Preparation and downloading only take a few moments per system.

### What about multi-disc games or zipped ROMs?

Matching is based on the file name without its extension, so `Game (Disc 1).cue`, `Game (Disc 1).zip`, and
`Game (Disc 1).chd` all match the cover `Game (Disc 1).png` if one exists. Compressed ROMs are supported
because the archive is never opened — only its name is used.

### Does it scan subfolders?

No. Only the files directly inside the selected ROM folder are scanned. Point the ROM folder at each
subfolder separately if your collection is organized that way.

### Does it work offline?

Cover downloads require an internet connection. The systems list is cached, so the application can start and
show systems offline, but covers cannot be fetched without access to GitHub.

### How do I update to a new version?

The application checks for new releases at startup and via **Help → Check for Updates**. When an update is
available, a banner links to the release page. Replace the extracted files with the new bundle — your
settings are stored separately in `%LocalAppData%` and are preserved.

## Data and privacy

### Where are my settings and logs?

Everything lives in `%LocalAppData%\RetroGameCoverDownloader`. Use **File → Open AppData Path** to open the
folder. See [Configuration](configuration.md#settings-storage) for the full list.

### Is my GitHub token safe?

The token is stored in `settings.dat`, encrypted with Windows DPAPI scoped to your Windows account. It cannot
be decrypted by another user or on another machine.

### What is sent to the developer?

Anonymous launch telemetry (application id and version) and error diagnostics when something fails. ROM
names, folder paths, and game names are not collected for analytics; they can appear inside error logs that
are reported only when an actual error occurs. Since version 1.5.0, transient network failures, timeouts,
rate limits, and cancellations are no longer reported. See [Privacy](configuration.md#privacy).

### How do I uninstall the application?

Delete the extracted application folder and, to remove all local data, the
`%LocalAppData%\RetroGameCoverDownloader` folder.

## Troubleshooting shortcuts

| Symptom | Where to look |
| --- | --- |
| Empty system list | [Troubleshooting](troubleshooting.md#the-system-list-is-empty) |
| Repeated token prompts | [Troubleshooting](troubleshooting.md#the-application-asks-for-a-token-repeatedly) |
| Rate limit countdown | [Troubleshooting](troubleshooting.md#rate-limit-reached-resuming-in-n-seconds) |
| Covers not matched | [Troubleshooting](troubleshooting.md#covers-are-not-found-during-prepare) |
| Download failures | [Troubleshooting](troubleshooting.md#downloads-fail) |

## Still have questions?

- Open a discussion or issue:
  <https://github.com/purelogiccode/RetroGameCoverDownloader/discussions>
- Email: <support@purelogiccode.com>
