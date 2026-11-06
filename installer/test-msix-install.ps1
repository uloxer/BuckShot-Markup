<# Installs and launches only on a disposable CI runner; never changes a developer's certificate trust. #>
param([string]$PackagePath = (Join-Path $PSScriptRoot 'BuckShot-Markup.msix'),
      [string]$CertificatePath = (Join-Path $PSScriptRoot 'BuckShot-Markup-Development.cer'),
      [string]$ExpectedVersion)
$ErrorActionPreference = 'Stop'
if ($env:GITHUB_ACTIONS -ne 'true') { throw 'MSIX installation and certificate trust tests require a disposable CI runner.' }
if (-not $ExpectedVersion) { $ExpectedVersion = (Get-Content (Join-Path $PSScriptRoot '../src/version.json') -Raw | ConvertFrom-Json).version }
$name = 'BuckShotMarkup.Preview'
if (Get-AppxPackage -Name $name) { throw 'An existing package must not be replaced by this test.' }
$PackagePath = (Resolve-Path -LiteralPath $PackagePath).Path
$certificate = Import-Certificate -FilePath $CertificatePath -CertStoreLocation Cert:\LocalMachine\TrustedPeople
$installed = $null; $appProcess = $null
try {
    if ((Get-AuthenticodeSignature -FilePath $PackagePath).Status -ne 'Valid') { throw 'MSIX signature verification failed.' }
    Add-AppxPackage -Path $PackagePath
    $installed = Get-AppxPackage -Name $name
    if (-not $installed -or $installed.Version -ne "$ExpectedVersion.0") { throw 'Per-user package registration/version is wrong.' }
    if ($installed.Publisher -ne 'CN=BuckShot Markup Development') { throw 'Unexpected MSIX publisher.' }
    if (Test-Path (Join-Path $installed.InstallLocation 'Plugins/Greenshot.Plugin.RecipeEditor')) { throw 'Recipe Editor must not be packaged.' }
    foreach ($file in @('Greenshot.exe', 'greenshot-cli.exe', 'greenshot-proxy.exe')) {
        $info = [Diagnostics.FileVersionInfo]::GetVersionInfo((Join-Path $installed.InstallLocation $file))
        if ($info.ProductName -ne 'BuckShot Markup' -or ([version]$info.FileVersion).ToString(3) -ne $ExpectedVersion) { throw "Wrong packaged product/version: $file" }
    }
    Push-Location $installed.InstallLocation
    try { & (Join-Path $PSScriptRoot 'validate-installation.ps1') -WarningAction Stop }
    finally { Pop-Location }
    # Activate through the package AUMID, not an unpackaged copy of the executable.
    Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class BuckShotMsixActivation {
  [ComImport, Guid("45BA127D-10A8-46EA-8AB7-56EA9078943C")] private class Manager { }
  [ComImport, Guid("2E941141-7F97-4756-BA1D-9DECDE894A3D"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  private interface IManager {
    [PreserveSig] int ActivateApplication([MarshalAs(UnmanagedType.LPWStr)] string id, [MarshalAs(UnmanagedType.LPWStr)] string args, uint options, out uint pid);
  }
  public static uint Launch(string id, string args) {
    uint pid; var manager = (IManager)new Manager();
    try { Marshal.ThrowExceptionForHR(manager.ActivateApplication(id, args, 0, out pid)); return pid; }
    finally { Marshal.ReleaseComObject(manager); }
  }
}
'@
    $defaultConfig = Join-Path $env:APPDATA 'BuckShot-Markup/buckshot-markup.ini'
    if (Test-Path $defaultConfig) { throw 'A clean runner with no existing BuckShot configuration is required.' }
    $appId = $installed.PackageFamilyName + '!App'
    $appProcessId = [BuckShotMsixActivation]::Launch($appId, '')
    Start-Sleep -Seconds 5
    $appProcess = Get-Process -Id $appProcessId -ErrorAction Stop
    if (-not $appProcess.Path.StartsWith($installed.InstallLocation, [StringComparison]::OrdinalIgnoreCase)) { throw 'A different application was activated.' }
    $cli = Join-Path $installed.InstallLocation 'greenshot-cli.exe'
    $versionOutput = & $cli --version 2>&1 | Out-String
    if ($LASTEXITCODE -ne 0 -or $versionOutput -notmatch [regex]::Escape($ExpectedVersion)) { throw "Packaged CLI/version smoke failed: $versionOutput" }
    & $cli --exit
    if (-not $appProcess.WaitForExit(15000)) { throw 'Packaged application did not exit cleanly.' }
    $privateData = Join-Path $env:LOCALAPPDATA ('Packages/' + $installed.PackageFamilyName)
    $privateConfig = @(Get-ChildItem $privateData -Filter buckshot-markup.ini -Recurse -File -ErrorAction SilentlyContinue)
    if (-not (Test-Path $defaultConfig) -and -not $privateConfig.Count) { throw 'Packaged app did not save default configuration to writable AppData.' }
    Write-Host "Verified signed per-user MSIX $($installed.PackageFullName), package activation, CLI and writable settings."
}
finally {
    if ($appProcess -and -not $appProcess.HasExited) { Stop-Process -Id $appProcess.Id -Force }
    if ($installed) { Remove-AppxPackage -Package $installed.PackageFullName }
    Remove-Item "Cert:\LocalMachine\TrustedPeople\$($certificate.Thumbprint)"
}
if (Get-AppxPackage -Name $name) { throw 'MSIX uninstall left the package registered.' }
Write-Host 'Verified current-user MSIX uninstall and removal of CI-only certificate trust.'
