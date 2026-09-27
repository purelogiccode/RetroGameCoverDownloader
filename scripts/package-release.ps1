<#
.SYNOPSIS
    Builds the RetroGameCoverDownloader release bundles.

.DESCRIPTION
    Publishes the app for x64 and arm64 as framework-dependent single-file executables
    and packages each one with ReadMe.md, LICENSE.txt and WhatsNew.md, following the
    release_<version>_win-<arch>.zip naming convention used by every published release.

    Existing files in the output directory are never deleted; only the two bundles for the
    requested version are created (or overwritten if they already exist for that version).
    The publish/staging directory is temporary and may be cleaned.

.PARAMETER Version
    Release version, e.g. 1.5.0.

.PARAMETER OutputDirectory
    Where the bundles are written. Defaults to RetroGameCoverDownloader\bin\Release, the
    folder that holds all historical bundles.

.PARAMETER StagingDirectory
    Temporary publish/staging root. Defaults to a folder under the system temp directory.

.PARAMETER SkipTests
    Skips the test run that normally guards a release build (CI runs tests in a
    separate job and passes this switch).

.EXAMPLE
    .\scripts\package-release.ps1 -Version 1.5.0
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $Version,

    [string] $OutputDirectory,

    [string] $StagingDirectory,

    [switch] $SkipTests
)

$ErrorActionPreference = 'Stop'

if ($Version -notmatch '^\d+\.\d+\.\d+$')
{
    throw "Invalid version '$Version' - expected a three-part version such as 1.5.0."
}

$repoRoot = Split-Path -Parent $PSScriptRoot
if (-not $OutputDirectory) { $OutputDirectory = Join-Path $repoRoot 'RetroGameCoverDownloader\bin\Release' }
if (-not $StagingDirectory) { $StagingDirectory = Join-Path ([System.IO.Path]::GetTempPath()) "RetroGameCoverDownloader-release-$Version" }

$project = 'RetroGameCoverDownloader\RetroGameCoverDownloader.csproj'
$exe = 'RetroGameCoverDownloader.exe'
$architectures = @('x64', 'arm64')
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

$bundles = @()
foreach ($architecture in $architectures)
{
    $rid = "win-$architecture"
    $publishDirectory = Join-Path $StagingDirectory $rid

    if (Test-Path -LiteralPath $publishDirectory) { Remove-Item -LiteralPath $publishDirectory -Recurse -Force }
    New-Item -ItemType Directory -Force -Path $publishDirectory | Out-Null

    Write-Host "Publishing $rid..."
    dotnet publish (Join-Path $repoRoot $project) -c Release -r $rid --self-contained false -p:PublishSingleFile=true -o $publishDirectory --nologo
    if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed for $rid." }

    # Bundle exactly the files shipped by every historical release: the single-file
    # executable plus the documentation the release process requires. The .NET runtime
    # is never bundled (the publish is framework-dependent).
    $bundleFiles = @()
    $runtimePath = Join-Path $publishDirectory $exe
    if (-not (Test-Path -LiteralPath $runtimePath))
    {
        throw "Publish output for $rid is missing '$exe'."
    }
    $bundleFiles += $runtimePath
    foreach ($document in $documents)
    {
        $bundleFiles += Join-Path $repoRoot $document
    }

    $bundleName = "release_${Version}_$rid.zip"
    $bundlePath = Join-Path $OutputDirectory $bundleName

    Compress-Archive -Path $bundleFiles -DestinationPath $bundlePath -Force
    $bundles += Get-Item -LiteralPath $bundlePath
    Write-Host "Created $bundlePath"
}

Write-Host ''
Write-Host 'Bundles:'
$bundles | ForEach-Object {
    $hash = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash
    '{0}  ({1:N0} bytes)  SHA256 {2}' -f $_.FullName, $_.Length, $hash
}
