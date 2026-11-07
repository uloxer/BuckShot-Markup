# Stage the native BuckShot build output; managed names now match filenames directly.
param([Parameter(Mandatory = $true)][string]$BuildOutput)
$ErrorActionPreference = 'Stop'
$payload = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot 'payload'))
if ((Split-Path $payload -Parent) -ne [IO.Path]::GetFullPath($PSScriptRoot)) { throw 'Invalid package staging path' }
if (Test-Path -LiteralPath $payload) { Remove-Item -LiteralPath $payload -Recurse -Force }
[void][IO.Directory]::CreateDirectory($payload)
Copy-Item -Path (Join-Path $BuildOutput '*') -Destination $payload -Recurse
$repository = Split-Path $PSScriptRoot -Parent
$languages = Join-Path $payload 'Languages'
[void][IO.Directory]::CreateDirectory($languages)
Copy-Item -Path (Join-Path $repository 'src/BuckShot/Languages/*.ini') -Destination $languages -Force
& (Join-Path $repository 'src/BuckShot-Installer/scripts/prune-plugin-files.ps1') -OutputDir $payload
$oldPaths = @(Get-ChildItem -LiteralPath $payload -Recurse | Where-Object { $_.Name -imatch 'greenshot' })
if ($oldPaths.Count) { throw "Unbranded package paths: $($oldPaths.FullName -join ', ')" }
Write-Host 'Prepared native BuckShot payload with matching managed assembly identities.'
