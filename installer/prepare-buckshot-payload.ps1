# Keep assembly identities (serialized projects and WPF pack URIs) while branding physical files.
param([Parameter(Mandatory = $true)][string]$BuildOutput)
$ErrorActionPreference = 'Stop'
$payload = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot 'payload'))
if ((Split-Path $payload -Parent) -ne [IO.Path]::GetFullPath($PSScriptRoot)) { throw 'Invalid package staging path' }
# This is the one dedicated, disposable package stage, never an installed application.
if (Test-Path -LiteralPath $payload) { Remove-Item -LiteralPath $payload -Recurse -Force }
[void][IO.Directory]::CreateDirectory($payload)
Copy-Item -Path (Join-Path $BuildOutput '*') -Destination $payload -Recurse
$repository = Split-Path $PSScriptRoot -Parent
$languages = Join-Path $payload 'Languages'
[void][IO.Directory]::CreateDirectory($languages)
Copy-Item -Path (Join-Path $repository 'src/Greenshot/Languages/*.ini') -Destination $languages -Force
foreach ($file in Get-ChildItem -LiteralPath $payload -Recurse -File) {
    if ($file.Name -imatch '^greenshot') {
        Rename-Item -LiteralPath $file.FullName -NewName ($file.Name -ireplace '^greenshot', 'BuckShot')
    }
}
foreach ($directory in Get-ChildItem -LiteralPath $payload -Recurse -Directory | Sort-Object { $_.FullName.Length } -Descending) {
    if ($directory.Name -imatch '^greenshot') {
        Rename-Item -LiteralPath $directory.FullName -NewName ($directory.Name -ireplace '^greenshot', 'BuckShot')
    }
}
# Remove duplicate shared assemblies before emitting bindings, so every mapped path ships.
& (Join-Path $repository 'src/Greenshot-Installer/scripts/prune-plugin-files.ps1') -OutputDir $payload
$configPath = Join-Path $payload 'BuckShot.exe.config'
[xml]$config = Get-Content -LiteralPath $configPath -Raw
$namespace = 'urn:schemas-microsoft-com:asm.v1'
$bindings = $config.configuration.runtime.assemblyBinding
# CLR codeBase supports private assemblies at relative application paths. Map every renamed
# managed DLL, including plugins, without changing their embedded legacy assembly identities.
foreach ($file in Get-ChildItem -LiteralPath $payload -Recurse -Filter 'BuckShot*.dll') {
    $identity = [Reflection.AssemblyName]::GetAssemblyName($file.FullName)
    $dependent = $config.CreateElement('dependentAssembly', $namespace)
    $assembly = $config.CreateElement('assemblyIdentity', $namespace)
    $assembly.SetAttribute('name', $identity.Name)
    $assembly.SetAttribute('culture', 'neutral')
    $token = ($identity.GetPublicKeyToken() | ForEach-Object { $_.ToString('x2') }) -join ''
    $assembly.SetAttribute('publicKeyToken', $(if ($token) { $token } else { 'null' }))
    [void]$dependent.AppendChild($assembly)
    $codeBase = $config.CreateElement('codeBase', $namespace)
    $codeBase.SetAttribute('version', $identity.Version.ToString())
    $codeBase.SetAttribute('href', $file.FullName.Substring($payload.Length + 1).Replace('\', '/'))
    [void]$dependent.AppendChild($codeBase)
    [void]$bindings.AppendChild($dependent)
}
$config.Save($configPath)
$oldPaths = @(Get-ChildItem -LiteralPath $payload -Recurse | Where-Object { $_.Name -imatch 'greenshot' })
if ($oldPaths.Count) { throw "Unbranded package paths: $($oldPaths.FullName -join ', ')" }
Write-Host 'Prepared BuckShot payload with preserved managed identities and explicit CLR bindings.'
