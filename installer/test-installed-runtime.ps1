param([Parameter(Mandatory = $true)][string]$InstallDirectory)
$ErrorActionPreference = 'Stop'
$probe = Join-Path $InstallDirectory 'BuckShot.InstallationProbe.exe'
$source = Join-Path $env:TEMP 'BuckShot.InstallationProbe.cs'
@'
using System;
using System.IO;
using System.Reflection;
class InstallationProbe {
    static int Main() {
        try {
            string root = AppDomain.CurrentDomain.BaseDirectory;
            var core = Assembly.Load(AssemblyName.GetAssemblyName(Path.Combine(root, "BuckShot.Base.dll")).FullName);
            var editor = Assembly.Load(AssemblyName.GetAssemblyName(Path.Combine(root, "BuckShot.Editor.dll")).FullName);
            if (Path.GetFileName(core.Location) != "BuckShot.Base.dll" ||
                Path.GetFileName(editor.Location) != "BuckShot.Editor.dll") throw new Exception("CLR binding failed");
            var app = Assembly.LoadFrom(Path.Combine(root, "BuckShot.exe"));
            if (app.GetName().Name != "Greenshot") throw new Exception("Legacy application identity changed");
            var texts = core.GetType("Greenshot.Base.Languages.Texts", true);
            texts.GetMethod("Initialize").Invoke(null, new object[] { new[] { Path.Combine(root, "Languages") } });
            object language = texts.GetProperty("Core").GetValue(null, null);
            string cancel = (string)core.GetType("Greenshot.Base.Languages.ICoreLanguage").GetProperty("Cancel").GetValue(language, null);
            if (string.IsNullOrEmpty(cancel) || cancel.StartsWith("###")) throw new Exception("Language loading failed");
            var binder = (System.Runtime.Serialization.SerializationBinder)Activator.CreateInstance(editor.GetType("Greenshot.Editor.Helpers.BinaryFormatterHelper", true), true);
            var legacyType = binder.BindToType("Greenshot.Base, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null", "Greenshot.Base.Interfaces.Drawing.EditStatus");
            object value = Enum.GetValues(legacyType).GetValue(0);
            using (var stream = new MemoryStream()) {
                var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter { Binder = binder };
                formatter.Serialize(stream, value);
                stream.Position = 0;
                if (!value.Equals(formatter.Deserialize(stream))) throw new Exception("Legacy project binder roundtrip failed");
            }
            int plugins = 0;
            foreach (string file in Directory.GetFiles(Path.Combine(root, "Plugins"), "BuckShot.Plugin.*.dll", SearchOption.AllDirectories)) {
                var plugin = Assembly.LoadFrom(file);
                string identity = plugin.GetName().Name;
                plugin.GetTypes();
                if (plugin.GetType(identity + "." + identity.Replace("Greenshot.Plugin.", "") + "Plugin") == null)
                    throw new Exception("Missing plugin entry: " + file);
                plugins++;
            }
            if (plugins < 8) throw new Exception("Missing full-install plugins: " + plugins);
            Console.WriteLine("Verified CLR bindings, preserved legacy identities, editor types, translated text and " + plugins + " plugin entry points.");
            return 0;
        } catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }
}
'@ | Set-Content -LiteralPath $source
$compiler = Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
try {
    & $compiler /nologo /target:exe "/out:$probe" $source
    if ($LASTEXITCODE -ne 0) { throw 'Runtime probe compilation failed' }
    Copy-Item -LiteralPath (Join-Path $InstallDirectory 'BuckShot.exe.config') -Destination "$probe.config"
    & $probe
    if ($LASTEXITCODE -ne 0) { throw 'Installed CLR/plugin/language probe failed' }
}
finally {
    Remove-Item -LiteralPath $source, $probe, "$probe.config" -ErrorAction SilentlyContinue
}
$app = Start-Process -FilePath (Join-Path $InstallDirectory 'BuckShot.exe') -WindowStyle Hidden -PassThru
try {
    Start-Sleep -Seconds 10
    if ($app.HasExited) { throw "Installed application exited during startup: $($app.ExitCode)" }
    $cli = Start-Process -FilePath (Join-Path $InstallDirectory 'buckshot-cli.exe') -ArgumentList '--exit' -WindowStyle Hidden -PassThru
    if (-not $cli.WaitForExit(30000)) { throw 'Renamed CLI timed out' }
    if ($cli.ExitCode -ne 0) { throw "Renamed CLI/IPC failed: $($cli.ExitCode)" }
    if (-not $app.WaitForExit(15000)) { throw 'CLI did not shut down the installed application' }
    Write-Host 'Verified installed BuckShot.exe startup and buckshot-cli.exe authenticated IPC shutdown.'
}
finally { if (-not $app.HasExited) { Stop-Process -Id $app.Id -Force } }
