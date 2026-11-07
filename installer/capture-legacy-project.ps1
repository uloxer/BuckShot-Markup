param([Parameter(Mandatory = $true)][string]$InstallerPath,
      [Parameter(Mandatory = $true)][string]$OutputDirectory)
$ErrorActionPreference = 'Stop'
if ($env:GITHUB_ACTIONS -ne 'true') { throw 'Legacy fixture capture requires a disposable CI runner' }
$appDir = Join-Path $env:LOCALAPPDATA 'Programs/BuckShot-LegacyFixture'
$installer = (Resolve-Path -LiteralPath $InstallerPath).Path
$process = Start-Process -FilePath $installer -ArgumentList "/CURRENTUSER /VERYSILENT /SUPPRESSMSGBOXES /NORESTART /SP- /TASKS=`"`" /TYPE=full /DIR=`"$appDir`"" -WindowStyle Hidden -Wait -PassThru
if ($process.ExitCode -ne 0) { throw 'Legacy fixture installation failed' }
try {
    $probe = Join-Path $appDir 'CaptureLegacyProject.exe'
    $compiler = Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
    $references = @(Get-ChildItem -LiteralPath $appDir -Filter '*.dll' | ForEach-Object { "/reference:$($_.FullName)" })
    & $compiler /nologo /target:exe "/out:$probe" /reference:System.Drawing.dll /reference:System.Windows.Forms.dll @references (Join-Path $PSScriptRoot 'compatibility/CaptureLegacyProject.cs')
    if ($LASTEXITCODE -ne 0) { throw 'Legacy fixture generator compilation failed' }
    Copy-Item -LiteralPath (Join-Path $appDir 'BuckShot.exe.config') -Destination "$probe.config"
    & $probe ([IO.Path]::GetFullPath($OutputDirectory))
    if ($LASTEXITCODE -ne 0) { throw 'Legacy fixture capture failed' }
    Remove-Item -LiteralPath $probe, "$probe.config"
}
finally {
    $process = Start-Process -FilePath (Join-Path $appDir 'unins000.exe') -ArgumentList '/VERYSILENT /SUPPRESSMSGBOXES /NORESTART' -WindowStyle Hidden -Wait -PassThru
    if ($process.ExitCode -ne 0) { throw 'Legacy fixture uninstall failed' }
}
