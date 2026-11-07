<# Wraps the already-built desktop payload. Does not install the package or trust certificates. #>
param(
    [string]$PayloadPath = (Join-Path $PSScriptRoot 'payload'),
    [string]$OutputDirectory = $PSScriptRoot,
    [string]$CertificateThumbprint,
    [switch]$CreateDevelopmentCertificate
)
$ErrorActionPreference = 'Stop'
function Find-SdkTool([string]$Name) {
    $command = Get-Command $Name -ErrorAction SilentlyContinue
    if ($command) { return $command.Source }
    $kits = Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Windows Kits\Installed Roots' -ErrorAction SilentlyContinue
    $roots = @((Join-Path ${env:ProgramFiles(x86)} 'Windows Kits/10/bin'))
    if ($kits.KitsRoot10) { $roots += Join-Path $kits.KitsRoot10 'bin' }
    foreach ($root in $roots | Select-Object -Unique) {
        foreach ($folder in Get-ChildItem $root -Directory -ErrorAction SilentlyContinue | Sort-Object Name -Descending) {
            $candidate = Join-Path $folder.FullName "x64/$Name"
            if (Test-Path -LiteralPath $candidate) { return $candidate }
        }
    }
    throw "$Name not found. Install the Windows 10/11 SDK signing and packaging tools."
}
function Invoke-Checked([string]$Program, [string[]]$Arguments) {
    & $Program @Arguments
    if ($LASTEXITCODE -ne 0) { throw "$Program failed with exit code $LASTEXITCODE" }
}
$makeAppx = Find-SdkTool 'MakeAppx.exe'
$signTool = Find-SdkTool 'SignTool.exe'
$PayloadPath = (Resolve-Path -LiteralPath $PayloadPath).Path
$OutputDirectory = [IO.Path]::GetFullPath($OutputDirectory)
[void][IO.Directory]::CreateDirectory($OutputDirectory)
$repository = Split-Path $PSScriptRoot -Parent
$version = (Get-Content (Join-Path $repository 'src/version.json') -Raw | ConvertFrom-Json).version
if (([version][Diagnostics.FileVersionInfo]::GetVersionInfo((Join-Path $PayloadPath 'BuckShot.exe')).FileVersion).ToString(3) -ne $version) {
    throw 'Build the current desktop application before building MSIX.'
}
$stage = Join-Path ([IO.Path]::GetTempPath()) ('BuckShot-MSIX-' + [guid]::NewGuid().ToString('N'))
[void][IO.Directory]::CreateDirectory($stage)
$createdCertificate = $null
try {
    # Reuse the validated runtime inventory, avoiding build tools, debug symbols and private files.
    foreach ($line in Get-Content (Join-Path $PayloadPath 'checksum.SHA256')) {
        $relative = ($line -split '  ', 2)[1]
        if (-not $relative -or $relative.Contains('..') -or [IO.Path]::IsPathRooted($relative)) { throw 'Invalid runtime inventory path.' }
        $target = Join-Path $stage $relative
        [void][IO.Directory]::CreateDirectory((Split-Path $target -Parent))
        Copy-Item -LiteralPath (Join-Path $PayloadPath $relative) -Destination $target
    }
    Copy-Item (Join-Path $PayloadPath 'log4net.xml'), (Join-Path $PayloadPath 'checksum.SHA256') $stage
    $languages = Join-Path $stage 'Languages'; [void][IO.Directory]::CreateDirectory($languages)
    if (Test-Path (Join-Path $PayloadPath 'Languages')) { Copy-Item (Join-Path $PayloadPath 'Languages/*.ini') $languages -Force }
    Copy-Item (Join-Path $repository 'src/BuckShot-Installer/additional_files/license.txt') $stage
    Copy-Item (Join-Path $repository 'src/BuckShot-Installer/additional_files/buckshot-readme.txt') (Join-Path $stage 'readme.txt')
    if (Test-Path (Join-Path $stage 'Plugins/BuckShot.Plugin.RecipeEditor')) { throw 'Recipe Editor must not be packaged.' }
    [xml]$manifest = Get-Content (Join-Path $PSScriptRoot 'msix/AppxManifest.xml') -Raw
    $manifest.Package.Identity.Version = "$version.0"
    $manifest.Save((Join-Path $stage 'AppxManifest.xml'))
    Add-Type -AssemblyName System.Drawing
    $assets = Join-Path $stage 'Assets'; [void][IO.Directory]::CreateDirectory($assets)
    $source = [Drawing.Image]::FromFile((Join-Path $repository 'assets/branding/rendered/buckshot-256.png'))
    try {
        foreach ($logo in @{ 'StoreLogo.png'=50; 'Square44x44Logo.png'=44; 'Square150x150Logo.png'=150 }.GetEnumerator()) {
            $image = [Drawing.Bitmap]::new($logo.Value, $logo.Value); $graphics = [Drawing.Graphics]::FromImage($image)
            try {
                $graphics.InterpolationMode = [Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
                $graphics.DrawImage($source, 0, 0, $logo.Value, $logo.Value)
                $image.Save((Join-Path $assets $logo.Key), [Drawing.Imaging.ImageFormat]::Png)
            }
            finally { $graphics.Dispose(); $image.Dispose() }
        }
    }
    finally { $source.Dispose() }
    $package = Join-Path $OutputDirectory 'BuckShot-Markup.msix'
    Invoke-Checked $makeAppx @('pack', '/d', $stage, '/p', $package, '/o')
    if ($CreateDevelopmentCertificate) {
        if ($env:GITHUB_ACTIONS -ne 'true') { throw 'Automatic development certificates are restricted to disposable CI runners.' }
        $createdCertificate = New-SelfSignedCertificate -Type Custom -Subject $manifest.Package.Identity.Publisher `
            -CertStoreLocation Cert:\CurrentUser\My -KeyUsage DigitalSignature -KeyExportPolicy NonExportable `
            -KeyAlgorithm RSA -KeyLength 2048 -HashAlgorithm SHA256 -NotAfter (Get-Date).AddYears(1) `
            -TextExtension @('2.5.29.37={text}1.3.6.1.5.5.7.3.3', '2.5.29.19={text}')
        $CertificateThumbprint = $createdCertificate.Thumbprint
    }
    if (-not $CertificateThumbprint) { throw 'Supply a signing certificate thumbprint, or use -CreateDevelopmentCertificate on CI.' }
    $certificate = Get-Item "Cert:\CurrentUser\My\$CertificateThumbprint"
    if ($certificate.Subject -ne $manifest.Package.Identity.Publisher) { throw 'Signing certificate subject must match manifest publisher.' }
    Invoke-Checked $signTool @('sign', '/fd', 'SHA256', '/sha1', $CertificateThumbprint, '/s', 'My', $package)
    $publicCertificate = Join-Path $OutputDirectory 'BuckShot-Markup-Development.cer'
    Export-Certificate -Cert $certificate -FilePath $publicCertificate | Out-Null
    foreach ($file in @($package, $publicCertificate)) {
        $hash = (Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash.ToLowerInvariant()
        "$hash  $([IO.Path]::GetFileName($file))" | Set-Content -LiteralPath "$file.sha256" -Encoding ascii
    }
    Copy-Item (Join-Path $repository 'docs/msix-install.md') (Join-Path $OutputDirectory 'MSIX-INSTALL.md') -Force
    Write-Host "Built signed $package ($version.0), development publisher $($certificate.Subject)."
}
finally {
    if ($createdCertificate) { Remove-Item "Cert:\CurrentUser\My\$($createdCertificate.Thumbprint)" -DeleteKey }
    # Only this script's unique temporary staging folder is removed.
    if ([IO.Path]::GetFullPath($stage).StartsWith([IO.Path]::GetFullPath([IO.Path]::GetTempPath()), [StringComparison]::OrdinalIgnoreCase)) {
        Remove-Item -LiteralPath $stage -Recurse -Force
    }
}
