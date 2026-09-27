[![C#](https://img.shields.io/badge/C%23-.NET%2010.0-blue.svg)](https://dotnet.microsoft.com/)
[![WPF](https://img.shields.io/badge/WPF-Desktop%20App-blue.svg)](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/)
[![GitHub](https://img.shields.io/badge/GitHub-API%20Integration-lightgrey.svg)](https://docs.github.com/en/rest)
[![CI](https://github.com/purelogiccode/RetroGameCoverDownloader/actions/workflows/ci.yml/badge.svg)](https://github.com/purelogiccode/RetroGameCoverDownloader/actions/workflows/ci.yml)
[![Docs](https://github.com/purelogiccode/RetroGameCoverDownloader/actions/workflows/docs.yml/badge.svg)](https://github.com/purelogiccode/RetroGameCoverDownloader/actions/workflows/docs.yml)
[![Release](https://img.shields.io/github/v/release/purelogiccode/RetroGameCoverDownloader?label=release&sort=semver)](https://github.com/purelogiccode/RetroGameCoverDownloader/releases)
[![Downloads](https://img.shields.io/github/downloads/purelogiccode/RetroGameCoverDownloader/total?label=downloads)](https://github.com/purelogiccode/RetroGameCoverDownloader/releases)
[![License](https://img.shields.io/github/license/purelogiccode/RetroGameCoverDownloader?label=license)](LICENSE.txt)
[![Platform](https://img.shields.io/badge/platform-Windows%2010%2B-0078D6?logo=windows)](https://www.microsoft.com/windows)
[![Pages](https://img.shields.io/badge/docs-GitHub%20Pages-2563eb)](https://purelogiccode.github.io/RetroGameCoverDownloader/)
[![Wiki](https://img.shields.io/badge/docs-GitHub%20Wiki-333333)](https://github.com/purelogiccode/RetroGameCoverDownloader/wiki)

# 🎮 Retro Game Cover Downloader

A sleek, modern WPF application that automatically downloads missing cover art for your retro game ROM collection from the official [libretro-thumbnails](https://github.com/libretro-thumbnails) GitHub repositories.

## 📖 Overview

Tired of manually hunting for game cover art? **Retro Game Cover Downloader** scans your ROM folders, identifies missing covers, and fetches them directly from libretro's extensive thumbnail database. With built-in rate limit handling, progress tracking, and error reporting, managing your retro gaming library has never been easier!

## Documentation

Full documentation is published in two places:

- **Documentation site**: <https://purelogiccode.github.io/RetroGameCoverDownloader/>
- **Wiki**: <https://github.com/purelogiccode/RetroGameCoverDownloader/wiki>

The sources live in the [`docs`](docs) folder and are built and published automatically by the
[`docs` workflow](.github/workflows/docs.yml).

## ✨ Features

🎮 **Multi-System Support** - Automatically detects available gaming systems from libretro's repositories  
📁 **Smart Folder Scanning** - Scans your ROMs and existing covers to find what's missing  
⬇️ **Batch Downloading** - Downloads all missing covers in one click with progress tracking  
⚡ **Rate Limit Management** - Intelligent handling of GitHub API limits with visual countdown timer  
🔐 **Token Integration** - Optional GitHub token support for 4,900 downloads/hour (vs 55 without)  
🔑 **Auto Token Prompt** - Automatically prompts for a new token on 401 Unauthorized errors  
🔄 **Update Checker** - Automatically notifies you of new versions  
🐛 **Error Reporting** - Automatic bug reports to help improve the application  
⏹️ **Cancellation Support** - Cancel operations anytime  
📊 **Progress UI** - Real-time progress bar and status messages  
🖥️ **CLI Support** - Command-line arguments for automation  
🌐 **Proxy Support** - Configure HTTP proxy with host, port, and authenticated credentials  
📂 **File Extensions Configuration** - Customize which file extensions are recognized as ROM files  
🔒 **Settings Encryption** - Tokens and passwords are encrypted using Windows DPAPI  
🔁 **Retry with Exponential Backoff** - Automatic retry on transient failures with intelligent backoff  
⚡ **Circuit Breaker** - Protects against hammering distressed servers during outages  
💾 **Systems Cache** - Caches the available systems list for faster startup and offline resilience  
🌙 **Modern UI** - Fluent Design dark theme powered by WPF-UI  
📸 **Screenshot Hotkey** - Press F8 anytime to capture the app window

## 🖼️ Screenshots

![Screenshot](screenshot.png)

## 📦 Installation

### Prerequisites
- Windows 10 or later
- .NET 10.0 Runtime (installed automatically with the application)

### Download
1. Grab the latest release from the [Releases Page](https://github.com/purelogiccode/RetroGameCoverDownloader/releases)
2. Extract the ZIP file to your desired location
3. Run `RetroGameCoverDownloader.exe`

## 🔧 Configuration

### GitHub Token (Recommended)
To unlock the full 4,900 downloads/hour limit:

1. Go to **File → GitHub Token** in the menu bar (the token dialog also appears automatically on first launch)
2. Follow the in-app instructions or:
    - Visit [GitHub Token Settings](https://github.com/settings/tokens/new)
    - Generate a token with **`public_repo`** scope
    - Copy and paste it into the application

> **💡 Tip**: Without a token, you're limited to ~55 covers/hour. With a token, you can download thousands!

## 🚀 Usage

### GUI Mode
1. **Set ROM Folder**: Browse to your ROM collection directory
2. **Set Cover Folder**: Choose where to save cover images
3. **Select System**: Pick your gaming system (NES, SNES, Genesis, etc.)
4. **Prepare**: Scan and identify missing covers
5. **Download**: Fetch all missing covers automatically

### Command-Line Mode
```bash
# Basic usage (ROM folder first, then Cover folder)
RetroGameCoverDownloader.exe "C:\ROMs\SNES" "C:\Covers\SNES"

# With flags
RetroGameCoverDownloader.exe --cover "C:\Covers" --rom "C:\ROMs"

# Positional arguments (ROM folder first, then Cover folder)
RetroGameCoverDownloader.exe "C:\ROMs" "C:\Covers"
```

## 🛠️ Technical Details

### Architecture
- **MVVM Pattern**: Main window uses ViewModels; dialogs use code-behind for simplicity
- **Async/Await**: Fully asynchronous operations for UI responsiveness
- **WPF-UI**: Modern Fluent Design dark theme with dynamic resources
- **Serilog**: Structured logging with buffered UI sink, file sink, and debug output
- **Rate Limiting**: Custom `RateLimiter` service with event notifications
- **Error Handling**: Comprehensive logging and bug reporting via BugReportSink
- **Circuit Breaker**: Tracks consecutive 503 errors and triggers cooldown to avoid hammering distressed servers
- **Retry Logic**: Exponential backoff with configurable retry attempts

### Key Components
- `MainViewModel`: Core business logic and state management
- `GitHubService`: Handles all GitHub API interactions
- `BugReportService` / `BugReportSink`: Automatic error reporting via structured Serilog sink
- `SettingsManager`: JSON-based settings persistence with DPAPI encryption for sensitive data
- `RateLimiter`: Intelligent API request throttling
- `UpdateCheckerService`: Checks for new application versions

### Testing
The project includes a comprehensive test suite using **xunit**:

```bash
dotnet test
```

Tests cover models, services, helpers, converters, commands, ViewModels, and managers. A `MockBugReportService` is injected via `[ModuleInitializer]` to prevent real API calls during testing.

## 📄 License

This project is licensed under the **GPL-3.0 license**.

## 🆘 Support

- **Issues**: [Report Bugs](https://github.com/purelogiccode/RetroGameCoverDownloader/issues)
- **Discussions**: [Ask Questions](https://github.com/purelogiccode/RetroGameCoverDownloader/discussions)
- **Email**: support@purelogiccode.com

## ⭐ Show Your Support

If you find this project helpful, please consider giving it a **star** on GitHub! It helps others discover the project and motivates continued development.

[![GitHub Stars](https://img.shields.io/github/stars/purelogiccode/RetroGameCoverDownloader?style=social)](https://github.com/purelogiccode/RetroGameCoverDownloader)

**⭐ Click the star button at the top of the repository if you like this project! ⭐**

---

<div align="center">
Made with ❤️ by <a href="https://www.purelogiccode.com">Pure Logic Code</a>
</div>