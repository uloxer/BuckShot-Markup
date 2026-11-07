<# Run only on a disposable Windows build runner: exercises the real per-user installer and uninstaller. #>
param(
    [Parameter(Mandatory = $true)][string]$InstallerPath,
    [string]$ExpectedVersion = '',
    [string]$PreviousInstallerPath = ''
)
$ErrorActionPreference = 'Stop'
if ($env:GITHUB_ACTIONS -ne 'true') { throw 'Use a disposable GitHub Actions runner for installation tests.' }
$installer = (Resolve-Path -LiteralPath $InstallerPath).Path
$installDir = Join-Path $env:LOCALAPPDATA 'Programs/BuckShot-Markup'
$userKey = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\BuckShot-Markup_is1'
$machineKey = 'HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\BuckShot-Markup_is1'
if ((Test-Path $userKey) -or (Test-Path $machineKey) -or (Test-Path -LiteralPath $installDir)) {
    throw 'Runner already has BuckShot Markup; refusing to change an existing installation.'
}
# Disposable-runner sentinels exercise coexistence with the upstream product.
$upstreamConfig = Join-Path $env:APPDATA 'Greenshot/greenshot.ini'
$upstreamConfigExisted = Test-Path -LiteralPath $upstreamConfig
if (-not $upstreamConfigExisted) {
    [void][IO.Directory]::CreateDirectory((Split-Path $upstreamConfig -Parent))
    [IO.File]::WriteAllText($upstreamConfig, '; Greenshot coexistence probe: must remain untouched')
}
$upstreamHash = (Get-FileHash -LiteralPath $upstreamConfig -Algorithm SHA256).Hash
$upstreamRegistration = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\Greenshot_is1'
$upstreamRegistrationExisted = Test-Path $upstreamRegistration
if (-not $upstreamRegistrationExisted) {
    [void](New-Item -Path $upstreamRegistration -Force)
    [void](New-ItemProperty -Path $upstreamRegistration -Name DisplayName -Value 'Greenshot coexistence probe' -PropertyType String)
}
$upstreamDisplayName = (Get-ItemProperty $upstreamRegistration).DisplayName
# Sentinel Classes defaults must be restored, without touching separate ProgIDs or UserChoice.
$associationDefaults = @{}
foreach ($extension in @('buckshot', 'greenshot')) {
    $key = "HKCU:\Software\Classes\.$extension"
    if (Test-Path $key) { throw "Runner already has $extension association; refusing to change it" }
    [void](New-Item -Path $key -Force)
    Set-Item -Path $key -Value 'Greenshot.CoexistenceProbe'
    $associationDefaults[$extension] = 'Greenshot.CoexistenceProbe'
}
if ($PreviousInstallerPath) {
    $previous = (Resolve-Path -LiteralPath $PreviousInstallerPath).Path
    $upgrade = Start-Process -FilePath $previous -ArgumentList '/CURRENTUSER /VERYSILENT /SUPPRESSMSGBOXES /NORESTART /SP- /TASKS="" /TYPE=full' -WindowStyle Hidden -Wait -PassThru
    if ($upgrade.ExitCode -ne 0) { throw 'Previous release installation failed' }
    if (-not (Test-Path (Join-Path $installDir 'Greenshot.exe'))) { throw 'Previous release branding fixture missing' }
    [IO.File]::WriteAllText((Join-Path $installDir 'upgrade-user-data.txt'), 'preserve user files')
    $settingsPath = Join-Path $env:APPDATA 'BuckShot-Markup/buckshot-markup.ini'
    [void][IO.Directory]::CreateDirectory((Split-Path $settingsPath -Parent))
    if (-not (Test-Path -LiteralPath $settingsPath)) { [IO.File]::WriteAllText($settingsPath, "[Core]
Language=en-US
") }
    $settingsHash = (Get-FileHash -LiteralPath $settingsPath -Algorithm SHA256).Hash
    Write-Host 'Installed 0.9.672 to exercise a real in-place upgrade.'
}
$process = Start-Process -FilePath $installer -ArgumentList '/CURRENTUSER /VERYSILENT /SUPPRESSMSGBOXES /NORESTART /SP- /TASKS="" /TYPE=full' -WindowStyle Hidden -Wait -PassThru
if ($process.ExitCode -ne 0) { throw "Installer failed: $($process.ExitCode)" }
try {
    if (-not (Test-Path $userKey)) { throw 'Per-user uninstall registration is missing' }
    if (Test-Path $machineKey) { throw 'Installer unexpectedly registered a machine-wide installation' }
    foreach ($extension in @('buckshot', 'greenshot')) {
        $key = "HKCU:\Software\Classes\.$extension"
        if ((Get-Item $key).GetValue('') -ne 'BuckShotMarkup.Project') { throw "Missing default for .$extension" }
        if (-not (Get-Item "$key\OpenWithProgids").GetValueNames().Contains('BuckShotMarkup.Project')) { throw "Missing OpenWith for .$extension" }
    }
    $command = (Get-Item 'HKCU:\Software\Classes\BuckShotMarkup.Project\shell\open\command').GetValue('')
    if ($command -notlike '*buckshot-proxy.exe*--file*%1*') { throw "Unexpected project open command: $command" }
    $registration = Get-ItemProperty $userKey
    if ($registration.DisplayName -ne 'BuckShot Markup') { throw "Unexpected product name: $($registration.DisplayName)" }
    if ($ExpectedVersion -and $registration.DisplayVersion -ne $ExpectedVersion) {
        throw "Unexpected installed version: $($registration.DisplayVersion); expected $ExpectedVersion"
    }
    $actualDir = $registration.InstallLocation.TrimEnd('\')
    if ($actualDir -ine $installDir) { throw "Unexpected user installation directory: $actualDir" }
    foreach ($file in @('BuckShot.exe', 'BuckShot.Base.dll', 'BuckShot.Editor.dll', 'buckshot-proxy.exe', 'buckshot-cli.exe', 'Plugins/BuckShot.Plugin.Stamps/BuckShot.Plugin.Stamps.dll', 'Languages/buckshot.stamps.en-US.ini')) {
        if (-not (Test-Path -LiteralPath (Join-Path $actualDir $file))) { throw "Missing installed file: $file" }
    }
    if (Test-Path -LiteralPath (Join-Path $actualDir 'Plugins/BuckShot.Plugin.RecipeEditor')) { throw 'Recipe Editor must not be distributed' }
    $oldPaths = @(Get-ChildItem -LiteralPath $actualDir -Recurse | Where-Object { $_.Name -imatch 'greenshot' })
    if ($oldPaths.Count) { throw "Unbranded installed paths: $($oldPaths.FullName -join ', ')" }
    $shell = New-Object -ComObject WScript.Shell
    $shortcut = Join-Path ([Environment]::GetFolderPath('Programs')) 'BuckShot Markup/BuckShot Markup.lnk'
    if ($shell.CreateShortcut($shortcut).TargetPath -ine (Join-Path $actualDir 'BuckShot.exe')) { throw 'Shortcut does not target BuckShot.exe' }
    if ($PreviousInstallerPath -and (Get-Content (Join-Path $actualDir 'upgrade-user-data.txt') -Raw) -ne 'preserve user files') { throw 'Upgrade removed user data' }
    if ($PreviousInstallerPath -and (Get-FileHash -LiteralPath $settingsPath -Algorithm SHA256).Hash -ne $settingsHash) { throw 'Upgrade modified existing settings' }
    & (Join-Path $PSScriptRoot 'test-installed-runtime.ps1') -InstallDirectory $actualDir
    $sourceVersion = (Get-Content -LiteralPath (Join-Path $PSScriptRoot '../src/version.json') -Raw | ConvertFrom-Json).version
    if ($ExpectedVersion -and $ExpectedVersion -ne $sourceVersion) { throw 'Requested and source versions differ' }
    foreach ($file in @('BuckShot.exe', 'buckshot-proxy.exe', 'buckshot-cli.exe')) {
        $info = [Diagnostics.FileVersionInfo]::GetVersionInfo((Join-Path $actualDir $file))
        if ($info.ProductName -ne 'BuckShot Markup') { throw "Unexpected product name in ${file}: $($info.ProductName)" }
        if ($file -eq 'BuckShot.exe' -and $info.FileDescription -ne 'BuckShot Markup') { throw "Unexpected Windows display name: $($info.FileDescription)" }
        if (([version]$info.FileVersion).ToString(3) -ne $sourceVersion) { throw "Unexpected file version in ${file}: $($info.FileVersion)" }
        # NBGV keeps a commit-height fourth component for the managed app; native helpers use a fixed zero revision.
        if ($file -ne 'BuckShot.exe' -and [version]$info.FileVersion -ne [version]"$sourceVersion.0") { throw "Unexpected native version revision in ${file}: $($info.FileVersion)" }
        if (([version]($info.ProductVersion -split '\+')[0]).ToString(3) -ne $sourceVersion) { throw "Unexpected product version in ${file}: $($info.ProductVersion)" }
        Write-Host "Verified $file product '$($info.ProductName)' version $($info.FileVersion)."
    }
    Push-Location $actualDir
    try { & (Join-Path $PSScriptRoot 'validate-installation.ps1') -WarningAction Stop }
    finally { Pop-Location }
    # A choice changed after installation must survive uninstall.
    Set-Item -Path 'HKCU:\Software\Classes\.buckshot' -Value 'UserOverride.CoexistenceProbe'
    $associationDefaults['buckshot'] = 'UserOverride.CoexistenceProbe'
    Write-Host "Verified per-user installation in $actualDir with no HKLM uninstall entry."
}
finally {
    $uninstaller = Join-Path $installDir 'unins000.exe'
    if (Test-Path -LiteralPath $uninstaller) {
        $process = Start-Process -FilePath $uninstaller -ArgumentList '/VERYSILENT /SUPPRESSMSGBOXES /NORESTART' -WindowStyle Hidden -Wait -PassThru
        if ($process.ExitCode -ne 0) { throw "Uninstaller failed: $($process.ExitCode)" }
    }
}
if (Test-Path $userKey) { throw 'Uninstaller left the uninstall registration behind' }
if (Test-Path -LiteralPath (Join-Path $installDir 'BuckShot.exe')) { throw 'Uninstaller left the application behind' }
if (-not (Test-Path -LiteralPath $upstreamConfig) -or (Get-FileHash -LiteralPath $upstreamConfig -Algorithm SHA256).Hash -ne $upstreamHash) { throw 'BuckShot modified Greenshot configuration' }
if (-not (Test-Path $upstreamRegistration) -or (Get-ItemProperty $upstreamRegistration).DisplayName -ne $upstreamDisplayName) { throw 'BuckShot modified Greenshot registration' }
if (-not $upstreamConfigExisted) { Remove-Item -LiteralPath $upstreamConfig }
if (-not $upstreamRegistrationExisted) { Remove-Item -LiteralPath $upstreamRegistration }
Write-Host 'Verified Greenshot coexistence and independent BuckShot identity.'
foreach ($extension in @('buckshot', 'greenshot')) {
    $key = "HKCU:\Software\Classes\.$extension"
    if ((Get-Item $key).GetValue('') -ne $associationDefaults[$extension]) { throw "Uninstall did not restore .$extension default" }
    Remove-Item -LiteralPath $key -Recurse
}
Write-Host 'Verified per-user uninstall and restoration of both Classes defaults (Windows UserChoice unchanged).'
