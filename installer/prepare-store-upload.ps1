<# Repackages the validated release payload; installs a temporary signed copy only on disposable CI. #>
param(
    [Parameter(Mandatory = $true)][string]$SourcePackagePath,
    [Parameter(Mandatory = $true)][string]$OutputDirectory
)
$ErrorActionPreference = 'Stop'
if ($env:GITHUB_ACTIONS -ne 'true') { throw 'Store identity installation checks require a disposable CI runner.' }
$SourcePackagePath = (Resolve-Path -LiteralPath $SourcePackagePath).Path
$expectedSourceHash = 'd799e3a2c42cfe669ed8162118cf169307cf7f35c6e715986e97a3786a51a40b'
if ((Get-FileHash -LiteralPath $SourcePackagePath -Algorithm SHA256).Hash.ToLowerInvariant() -ne $expectedSourceHash) { throw 'Source must be the validated published 0.9.674 development MSIX.' }
function Find-StoreSdkTool([string]$Name) {
    $root = Join-Path ${env:ProgramFiles(x86)} 'Windows Kits/10/bin'
    foreach ($folder in Get-ChildItem -LiteralPath $root -Directory | Sort-Object Name -Descending) {
        $tool = Join-Path $folder.FullName "x64/$Name"
        if (Test-Path -LiteralPath $tool) { return $tool }
    }
    throw "Windows SDK tool unavailable: $Name"
}
$makeAppx = Find-StoreSdkTool 'MakeAppx.exe'
$signTool = Find-StoreSdkTool 'SignTool.exe'
$workspace = Join-Path ([IO.Path]::GetTempPath()) ('BuckShot-Store-' + [guid]::NewGuid().ToString('N'))
[void][IO.Directory]::CreateDirectory($workspace)
$certificate = $null
try {
    $payload = Join-Path $workspace 'payload'
    & $makeAppx unpack /p $SourcePackagePath /d $payload /o
    if ($LASTEXITCODE) { throw 'Source package unpack failed.' }
    Push-Location $payload
    try { & (Join-Path $PSScriptRoot 'validate-installation.ps1') -WarningAction Stop }
    finally { Pop-Location }
    & (Join-Path $PSScriptRoot 'build-msix.ps1') -PayloadPath $payload -OutputDirectory $OutputDirectory -StoreUpload
    $package = Join-Path ([IO.Path]::GetFullPath($OutputDirectory)) 'BuckShot-Markup-Store.msix'
    $unsignedHash = (Get-FileHash -LiteralPath $package -Algorithm SHA256).Hash
    $inspection = Join-Path $workspace 'inspection'
    & $makeAppx unpack /p $package /d $inspection /o
    if ($LASTEXITCODE) { throw 'Store package unpack failed.' }
    [xml]$manifest = Get-Content -LiteralPath (Join-Path $inspection 'AppxManifest.xml') -Raw
    $publisher = 'CN=39BB6DBD-CEE0-4BD3-A6D6-E8B3D7061493'
    if ($manifest.Package.Identity.Name -ne 'ULoXer.BuckShotMarkup' -or $manifest.Package.Identity.Publisher -ne $publisher -or
        $manifest.Package.Identity.Version -ne '0.9.674.0' -or $manifest.Package.Identity.ProcessorArchitecture -ne 'x64' -or
        $manifest.Package.Properties.PublisherDisplayName -ne 'ULoXer') { throw 'Store manifest identity mismatch.' }
    if (Test-Path -LiteralPath (Join-Path $inspection 'AppxSignature.p7x')) { throw 'Store upload must be unsigned.' }
    if (Get-ChildItem -LiteralPath $inspection -Recurse | Where-Object { $_.Name -imatch 'greenshot' }) { throw 'Obsolete packaged filename.' }
    $fileTypes = @($manifest.SelectNodes("//*[local-name()='FileType']") | ForEach-Object { $_.InnerText })
    foreach ($extension in @('.buckshot', '.greenshot')) { if ($fileTypes -notcontains $extension) { throw "Missing $extension association." } }
    if ($manifest.SelectSingleNode("//*[local-name()='ExecutionAlias']").Alias -ne 'buckshot-markup-cli.exe') { throw 'Wrong CLI alias.' }
    $plugins = @(Get-ChildItem -LiteralPath (Join-Path $inspection 'Plugins') -Recurse -Filter 'BuckShot.Plugin.*.dll')
    if ($plugins.Count -ne 8) { throw 'Expected eight bundled plugins.' }
    $inventory = @(Get-Content -LiteralPath (Join-Path $inspection 'checksum.SHA256') | Where-Object { $_.Trim() })
    if ($inventory.Count -ne 60) { throw 'Expected 60 validated runtime files.' }
    Push-Location $inspection
    try { & (Join-Path $PSScriptRoot 'validate-installation.ps1') -WarningAction Stop }
    finally { Pop-Location }
    # Keep the upload package unsigned. Only this separate disposable copy is signed.
    $testCopy = Join-Path $workspace 'Store-identity-test.msix'
    Copy-Item -LiteralPath $package -Destination $testCopy
    $certificate = New-SelfSignedCertificate -Type Custom -Subject $publisher -CertStoreLocation Cert:\CurrentUser\My `
        -KeyUsage DigitalSignature -KeyExportPolicy NonExportable -KeyAlgorithm RSA -KeyLength 2048 -HashAlgorithm SHA256 `
        -NotAfter (Get-Date).AddDays(1) -TextExtension @('2.5.29.37={text}1.3.6.1.5.5.7.3.3', '2.5.29.19={text}')
    & $signTool sign /fd SHA256 /sha1 $certificate.Thumbprint /s My $testCopy
    if ($LASTEXITCODE) { throw 'Temporary Store identity signing failed.' }
    $publicCertificate = Join-Path $workspace 'Store-identity-test.cer'
    Export-Certificate -Cert $certificate -FilePath $publicCertificate | Out-Null
    & (Join-Path $PSScriptRoot 'test-msix-install.ps1') -PackagePath $testCopy -CertificatePath $publicCertificate -ExpectedVersion '0.9.674' `
        -ExpectedName 'ULoXer.BuckShotMarkup' -ExpectedPublisher $publisher -ExpectedFamilyName 'ULoXer.BuckShotMarkup_a54b1ynwgtqsa'
    if ((Get-FileHash -LiteralPath $package -Algorithm SHA256).Hash -ne $unsignedHash) { throw 'Unsigned upload package changed during validation.' }
    Write-Host 'Verified unsigned Store upload: exact identity/PFN, 60 runtime hashes, eight plugins, associations and alias; separate signed-copy activation passed.'
}
finally {
    if ($certificate) { Remove-Item "Cert:\CurrentUser\My\$($certificate.Thumbprint)" -DeleteKey }
    $temporaryRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
    if (-not [IO.Path]::GetFullPath($workspace).StartsWith($temporaryRoot, [StringComparison]::OrdinalIgnoreCase)) { throw 'Unsafe temporary cleanup path.' }
    Remove-Item -LiteralPath $workspace -Recurse -Force
}
