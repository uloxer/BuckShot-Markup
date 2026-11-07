<#
.SYNOPSIS
    Creates the portable ZIP files (full and light) next to the installers of a local Release build.

.DESCRIPTION
    Uses prepare-portable.ps1, like the release workflow. The ZIP files are named after the installer that was just
    built: BuckShot-INSTALLER-<version>.exe gives BuckShot-PORTABLE-<version>.zip and BuckShot-Light-PORTABLE-<version>.zip.
    BuckShot Light is its own build of BuckShot.exe (bin\Release-Light, see BuckShot.csproj).
#>
param(
    [Parameter(Mandatory = $true)]
    [string]$RepositoryRootPath,
    [Parameter(Mandatory = $true)]
    [string]$BuildArtifactsPath,
    [Parameter(Mandatory = $true)]
    [string]$LightBuildArtifactsPath,
    [Parameter(Mandatory = $true)]
    [string]$InstallerDir
)

$ErrorActionPreference = 'Stop'

$installer = Get-ChildItem -Path $InstallerDir -Filter 'BuckShot-INSTALLER-*.exe' | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if (-not $installer) {
    throw "No BuckShot-INSTALLER-*.exe found in $InstallerDir"
}
$version = $installer.BaseName.Substring('BuckShot-INSTALLER-'.Length)

$prepareScript = Join-Path $RepositoryRootPath 'prepare-portable.ps1'
foreach ($edition in @(@{ Prefix = 'BuckShot'; Light = $false; Path = $BuildArtifactsPath }, @{ Prefix = 'BuckShot-Light'; Light = $true; Path = $LightBuildArtifactsPath })) {
    $staging = Join-Path ([System.IO.Path]::GetTempPath()) ("buckshot-portable-" + [guid]::NewGuid().ToString('N'))
    try {
        & $prepareScript -RepositoryRootPath $RepositoryRootPath -BuildArtifactsPath $edition.Path -OutputPath $staging -Light:$edition.Light
        $zip = Join-Path $InstallerDir ("{0}-PORTABLE-{1}.zip" -f $edition.Prefix, $version)
        Compress-Archive -Path (Join-Path $staging '*') -DestinationPath $zip -Force
        Write-Host "Created $zip"
    }
    finally {
        Remove-Item -Path $staging -Recurse -Force -ErrorAction SilentlyContinue
    }
}
