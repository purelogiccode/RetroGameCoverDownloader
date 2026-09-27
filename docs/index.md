# Retro Game Cover Downloader

A modern Windows desktop application that automatically downloads missing cover art for your retro game
ROM collection from the official [libretro-thumbnails](https://github.com/libretro-thumbnails) repositories.

![Retro Game Cover Downloader](https://raw.githubusercontent.com/purelogiccode/RetroGameCoverDownloader/master/screenshot.png)

## Key features

- **Multi-system support** — discovers the available systems directly from libretro's thumbnail repositories.
- **Smart scanning** — compares your ROM files with the covers you already have and finds exactly what is missing.
- **Batch downloading** — downloads every missing cover in one click, with live progress and status messages.
- **Rate limit aware** — throttles requests to GitHub, falls back to cached data when limits are hit, and unlocks
  4,900 requests/hour when a personal access token is configured (55/hour without).
- **Resilient networking** — exponential backoff retries, a 503 circuit breaker, branch fallback
  (`main` → `master`), and a Contents API fallback for `raw.githubusercontent.com`.
- **Secure settings** — your GitHub token and proxy password are encrypted with Windows DPAPI.
- **Modern interface** — Fluent Design dark theme built on [WPF-UI](https://github.com/lepoco/wpfui).
- **Diagnostics built in** — structured logging, local error logs, crash reporting, and a background update checker.

## Get started

1. [Install the application](guide/getting-started.md) on your Windows PC.
2. Follow the [User Guide](guide/user-guide.md) to set your folders and download covers.
3. Configure a [GitHub token](guide/configuration.md) to unlock the full download rate.
4. Stuck? See [Troubleshooting](guide/troubleshooting.md).

## Explore the documentation

| Page | What you will find |
| --- | --- |
| [Getting Started](guide/getting-started.md) | Requirements, installation, first run, and command-line usage |
| [User Guide](guide/user-guide.md) | The complete walkthrough of the interface and workflows |
| [Configuration](guide/configuration.md) | Token, proxy, file extensions, data locations, and privacy |
| [Architecture](guide/architecture.md) | How the application is built and how requests flow |
| [Troubleshooting](guide/troubleshooting.md) | Solutions for the most common problems |
| [Development Guide](guide/development.md) | Build, test, and contribute to the source code |
| [Release Process](guide/release-process.md) | Versioning, CI, packaging, and documentation publishing |
| [FAQ](guide/faq.md) | Frequently asked questions |

## Project links

- **Source code**: <https://github.com/purelogiccode/RetroGameCoverDownloader>
- **Releases**: <https://github.com/purelogiccode/RetroGameCoverDownloader/releases>
- **Issues**: <https://github.com/purelogiccode/RetroGameCoverDownloader/issues>
- **Discussions**: <https://github.com/purelogiccode/RetroGameCoverDownloader/discussions>
- **Website**: <https://www.purelogiccode.com>

## License

Retro Game Cover Downloader is released under the **GPL-3.0 license**. See `LICENSE.txt` in the repository
or download bundle for the full text.

---

Copyright &copy; Pure Logic Code. Made with care for the retro gaming community.
