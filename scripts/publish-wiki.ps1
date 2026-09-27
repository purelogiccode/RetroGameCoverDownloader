<#
.SYNOPSIS
    Publishes the Markdown documentation to a GitHub wiki working copy.

.DESCRIPTION
    Copies every Markdown file from the docs folder into the wiki working copy and
    generates the wiki's lateral menu (_Sidebar.md) from the navigation file.

    - index.md is published as Home.md, the wiki landing page.
    - Markdown links are rewritten to wiki-style links: [User Guide](guide/user-guide.md)
      becomes [User Guide](user-guide) because wiki pages are flat and resolve without
      the .md extension.
    - _Sidebar.md is generated from the toc.yml navigation file so every wiki page
      shows the lateral menu.
    - Wiki pages that do not correspond to a documentation file are left untouched.

    The wiki working copy must already exist; the docs workflow clones it before
    invoking this script.

.PARAMETER SourceDirectory
    Folder that contains the documentation Markdown files (for example "docs").

.PARAMETER WikiDirectory
    Working copy of the GitHub wiki (for example "wiki").

.PARAMETER NavigationFile
    The toc.yml used to generate _Sidebar.md. Defaults to guide/toc.yml inside the
    source directory, falling back to toc.yml in its root.

.EXAMPLE
    ./scripts/publish-wiki.ps1 -SourceDirectory docs -WikiDirectory wiki
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $SourceDirectory,

    [Parameter(Mandatory = $true)]
    [string] $WikiDirectory,

    [string] $NavigationFile
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $SourceDirectory -PathType Container))
{
    throw "Source directory '$SourceDirectory' was not found."
}

if (-not (Test-Path -LiteralPath $WikiDirectory -PathType Container))
{
    throw "Wiki directory '$WikiDirectory' was not found."
}

$source = (Resolve-Path -LiteralPath $SourceDirectory).Path
$wiki = (Resolve-Path -LiteralPath $WikiDirectory).Path

if (-not $NavigationFile)
{
    $candidates = @(
        (Join-Path $source 'guide/toc.yml'),
        (Join-Path $source 'toc.yml')
    )
    $NavigationFile = $candidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
}

function ConvertTo-WikiPageName {
    param([string] $Path)

    $name = [System.IO.Path]::GetFileNameWithoutExtension($Path)
    if ($name -eq 'index')
    {
        return 'Home'
    }

    return $name
}

$utf8NoBom = [System.Text.UTF8Encoding]::new($false)
$published = 0

$pages = Get-ChildItem -LiteralPath $source -Recurse -File -Filter '*.md' |
    Where-Object { $_.FullName -notmatch '[\\/]_site[\\/]' -and -not $_.Name.StartsWith('_') }

if ($pages.Count -eq 0)
{
    throw "No Markdown files were found in '$source'."
}

foreach ($page in $pages)
{
    $content = Get-Content -LiteralPath $page.FullName -Raw

    # The wiki landing page is always "Home".
    $content = $content -replace '\]\(index\.md(#[^)]*)?\)', '](Home$1)'

    # Wikis are flat and resolve pages without the .md extension. Absolute URLs are
    # left alone, and folder prefixes (for example guide/) are removed.
    $content = [regex]::Replace(
        $content,
        '\]\(([^)#]+?)\.md(#[^)]*)?\)',
        {
            param($match)
            $target = $match.Groups[1].Value
            if ($target.Contains('://', [System.StringComparison]::Ordinal))
            {
                return $match.Value
            }

            $target = $target -replace '^(\.\.?/|guide/)+', ''
            $pageName = [System.IO.Path]::GetFileNameWithoutExtension($target)
            if ($pageName -eq 'index') { $pageName = 'Home' }

            return '](' + $pageName + $match.Groups[2].Value + ')'
        })

    $targetName = ConvertTo-WikiPageName $page.Name
    $targetPath = Join-Path $wiki "$targetName.md"

    [System.IO.File]::WriteAllText($targetPath, $content, $utf8NoBom)
    Write-Host "Published $($page.Name) -> $targetName.md"
    $published++
}

$sidebarLines = [System.Collections.Generic.List[string]]::new()
$sidebarLines.Add('- [Home](Home)')

if ($NavigationFile -and (Test-Path -LiteralPath $NavigationFile))
{
    $currentName = $null
    foreach ($line in Get-Content -LiteralPath $NavigationFile)
    {
        if ($line -match '^\s*-\s*name:\s*(?<name>.+?)\s*$')
        {
            $currentName = $Matches['name'].Trim('"', "'")
            continue
        }

        if ($line -match '^\s*href:\s*(?<href>.+?)\s*$')
        {
            $href = $Matches['href'].Trim('"', "'")
            if ($currentName -and $href -match '\.md$')
            {
                $sidebarLines.Add("- [$currentName]($(ConvertTo-WikiPageName $href))")
            }

            $currentName = $null
        }
    }
}
else
{
    Write-Host "Navigation file not found; the wiki sidebar only links to Home."
}

[System.IO.File]::WriteAllText((Join-Path $wiki '_Sidebar.md'), ($sidebarLines -join [Environment]::NewLine) + [Environment]::NewLine, $utf8NoBom)
Write-Host "Generated _Sidebar.md with $($sidebarLines.Count) link(s)."

Write-Host "Published $published documentation page(s) to '$wiki'."
