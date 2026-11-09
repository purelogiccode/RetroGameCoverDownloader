<#
.SYNOPSIS
    Builds the RetroGameCoverDownloader release bundles for Windows, Linux, and macOS.

.DESCRIPTION
    Publishes the application as self-contained single-file executables for x64 and arm64
    on Windows, Linux, and macOS. Windows bundles are zip files; Linux bundles are tar.gz
    files; macOS bundles are tar.gz archives containing a .app bundle.

    The .NET runtime is included in every bundle. Existing files in the output directory
    are never deleted; only the bundles for the requested version are created or replaced.

.PARAMETER Version
    Release version, e.g. 1.6.0.

.PARAMETER OutputDirectory
    Where the bundles are written. Defaults to RetroGameCoverDownloader\bin\Release.

.PARAMETER StagingDirectory
    Temporary publish/staging root. Defaults to a folder under the system temp directory.

.PARAMETER Runtimes
    RIDs to build. Defaults to all six supported targets.

.PARAMETER SkipTests
    Skips the test run that normally guards a release build.

.EXAMPLE
    .\scripts\package-release.ps1 -Version 1.6.0

.EXAMPLE
    .\scripts\package-release.ps1 -Version 1.6.0 -Runtimes win-x64,linux-x64,osx-arm64
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $Version,

    [string] $OutputDirectory,

    [string] $StagingDirectory,

    [string[]] $Runtimes = @('win-x64', 'win-arm64', 'linux-x64', 'linux-arm64', 'osx-x64', 'osx-arm64'),

    [switch] $SkipTests
)

$ErrorActionPreference = 'Stop'

if ($Version -notmatch '^\d+\.\d+\.\d+$')
{
    throw "Invalid version '$Version' - expected a three-part version such as 1.6.0."
}

$isWindowsPlatform = [OperatingSystem]::IsWindows()
$repoRoot = Split-Path -Parent $PSScriptRoot
if (-not $OutputDirectory) { $OutputDirectory = Join-Path $repoRoot 'RetroGameCoverDownloader\bin\Release' }
if (-not $StagingDirectory) { $StagingDirectory = Join-Path ([System.IO.Path]::GetTempPath()) "RetroGameCoverDownloader-release-$Version" }

$project = Join-Path $repoRoot 'RetroGameCoverDownloader\RetroGameCoverDownloader.csproj'
$documents = @('ReadMe.md', 'LICENSE.txt', 'WhatsNew.md')

foreach ($document in $documents)
{
    if (-not (Test-Path -LiteralPath (Join-Path $repoRoot $document)))
    {
        throw "Required bundle file '$document' was not found in the repository root."
    }
}

New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
New-Item -ItemType Directory -Force -Path $StagingDirectory | Out-Null

if (-not $SkipTests)
{
    Write-Host 'Running tests before packaging...'
    dotnet test (Join-Path $repoRoot 'RetroGameCoverDownloader.Tests\RetroGameCoverDownloader.Tests.csproj') -c Release --nologo
    if ($LASTEXITCODE -ne 0) { throw 'Tests failed - release bundles were not created.' }
}

function Invoke-Tar {
    param([string] $ArchivePath, [string] $WorkingDirectory, [string[]] $Items)

    & tar -czf $ArchivePath -C $WorkingDirectory @Items
    if ($LASTEXITCODE -ne 0) { throw "tar failed while creating '$ArchivePath'." }
}

function Set-Executable {
    param([string] $Path)

    if (-not $isWindowsPlatform) {
        & chmod +x $Path
        if ($LASTEXITCODE -ne 0) { throw "chmod failed for '$Path'." }
    }
}

$bundles = @()

foreach ($rid in $Runtimes)
{
    if ($rid -notmatch '^(win|linux|osx)-(x64|arm64)$')
    {
        throw "Unsupported runtime '$rid'. Expected win-x64, win-arm64, linux-x64, linux-arm64, osx-x64, or osx-arm64."
    }

    $publishDirectory = Join-Path $StagingDirectory $rid
    if (Test-Path -LiteralPath $publishDirectory) { Remove-Item -LiteralPath $publishDirectory -Recurse -Force }
    New-Item -ItemType Directory -Force -Path $publishDirectory | Out-Null

    Write-Host "Publishing $rid..."
    dotnet publish $project -c Release -r $rid --self-contained true `
        -p:PublishSingleFile=true `
        -p:IncludeNativeLibrariesForSelfExtract=true `
        -p:EnableCompressionInSingleFile=true `
        -o $publishDirectory --nologo
    if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed for $rid." }

    # Native runtime PDBs are not needed in the shipped bundles.
    Get-ChildItem -LiteralPath $publishDirectory -Filter '*.pdb' -File | Remove-Item -Force

    foreach ($document in $documents)
    {
        Copy-Item -LiteralPath (Join-Path $repoRoot $document) -Destination $publishDirectory -Force
    }

    if ($rid.StartsWith('win-'))
    {
        $executable = Join-Path $publishDirectory 'RetroGameCoverDownloader.exe'
        if (-not (Test-Path -LiteralPath $executable))
        {
            throw "Publish output for $rid is missing 'RetroGameCoverDownloader.exe'."
        }

        $bundlePath = Join-Path $OutputDirectory "release_${Version}_$rid.zip"
        Compress-Archive -Path (Join-Path $publishDirectory '*') -DestinationPath $bundlePath -Force
    }
    elseif ($rid.StartsWith('linux-'))
    {
        $executable = Join-Path $publishDirectory 'RetroGameCoverDownloader'
        if (-not (Test-Path -LiteralPath $executable))
        {
            throw "Publish output for $rid is missing 'RetroGameCoverDownloader'."
        }

        Set-Executable $executable
        $bundlePath = Join-Path $OutputDirectory "release_${Version}_$rid.tar.gz"
        Invoke-Tar -ArchivePath $bundlePath -WorkingDirectory $publishDirectory -Items @(
            'RetroGameCoverDownloader', 'ReadMe.md', 'LICENSE.txt', 'WhatsNew.md')
    }
    else
    {
        $executable = Join-Path $publishDirectory 'RetroGameCoverDownloader'
        if (-not (Test-Path -LiteralPath $executable))
        {
            throw "Publish output for $rid is missing 'RetroGameCoverDownloader'."
        }

        $appDirectory = Join-Path $publishDirectory 'RetroGameCoverDownloader.app'
        $contentsDirectory = Join-Path $appDirectory 'Contents'
        $macOsDirectory = Join-Path $contentsDirectory 'MacOS'
        $resourcesDirectory = Join-Path $contentsDirectory 'Resources'
        New-Item -ItemType Directory -Force -Path $macOsDirectory, $resourcesDirectory | Out-Null

        $appExecutable = Join-Path $macOsDirectory 'RetroGameCoverDownloader'
        Copy-Item -LiteralPath $executable -Destination $appExecutable -Force
        Set-Executable $appExecutable

        foreach ($document in $documents)
        {
            Copy-Item -LiteralPath (Join-Path $repoRoot $document) -Destination $resourcesDirectory -Force
        }

        $infoPlist = @"
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleName</key>
    <string>Retro Game Cover Downloader</string>
    <key>CFBundleDisplayName</key>
    <string>Retro Game Cover Downloader</string>
    <key>CFBundleIdentifier</key>
    <string>com.purelogiccode.retrogamecoverdownloader</string>
    <key>CFBundleExecutable</key>
    <string>RetroGameCoverDownloader</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>CFBundleShortVersionString</key>
    <string>$Version</string>
    <key>CFBundleVersion</key>
    <string>$Version</string>
    <key>LSMinimumSystemVersion</key>
    <string>11.0</string>
    <key>NSHighResolutionCapable</key>
    <true/>
</dict>
</plist>
"@
        Set-Content -LiteralPath (Join-Path $contentsDirectory 'Info.plist') -Value $infoPlist -Encoding utf8NoBOM

        $bundlePath = Join-Path $OutputDirectory "release_${Version}_$rid.tar.gz"
        Invoke-Tar -ArchivePath $bundlePath -WorkingDirectory $publishDirectory -Items @('RetroGameCoverDownloader.app')
    }

    $bundles += Get-Item -LiteralPath $bundlePath
    Write-Host "Created $bundlePath"
}

Write-Host ''
Write-Host 'Bundles:'
$bundles | ForEach-Object {
    $hash = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash
    '{0}  ({1:N0} bytes)  SHA256 {2}' -f $_.FullName, $_.Length, $hash
}
