; Every program file by name, no wildcards: scripts\check-core-files.ps1 fails the build when the build output has one that
; isn't listed here (or the other way around), so nothing a package brings along ships by accident.
[Files]
Source: {#ReleaseDir}\BuckShot.exe; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\BuckShot.Base.dll; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\BuckShot.Editor.dll; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\BuckShot.exe.config; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\log4net.dll; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\log4net.xml; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\Dapplo.Ini.dll; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\Dapplo.Log.dll; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\Dapplo.Windows.dll; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\Dapplo.Windows.AppRestartManager.dll; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\Dapplo.Windows.Automation.dll; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\Dapplo.Windows.Clipboard.dll; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\Dapplo.Windows.Com.dll; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\Dapplo.Windows.Common.dll; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\Dapplo.Windows.DesktopWindowsManager.dll; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\Dapplo.Windows.Dpi.dll; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\Dapplo.Windows.Gdi32.dll; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\Dapplo.Windows.Icons.dll; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\Dapplo.Windows.Imaging.dll; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\Dapplo.Windows.Input.dll; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\Dapplo.Windows.Kernel32.dll; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\Dapplo.Windows.Messages.dll; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\Dapplo.Windows.Multimedia.dll; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\Dapplo.Windows.Shell32.dll; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\Dapplo.Windows.User32.dll; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\SixLabors.ImageSharp.dll; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\SixLabors.ImageSharp.Drawing.dll; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\SixLabors.Fonts.dll; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\System.Buffers.dll; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\System.CommandLine.dll; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\System.ComponentModel.Annotations.dll; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\System.IO.Pipelines.dll; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\System.Memory.dll; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\System.Numerics.Vectors.dll; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\System.Reactive.dll; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\System.Runtime.CompilerServices.Unsafe.dll; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\System.Text.Encoding.CodePages.dll; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\System.Text.Encodings.Web.dll; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\System.Text.Json.dll; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\System.Threading.Tasks.Extensions.dll; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\Svg.dll; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\ExCSS.dll; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\HtmlAgilityPack.dll; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\Hardcodet.NotifyIcon.Wpf.dll; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\Microsoft.Bcl.AsyncInterfaces.dll; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\Microsoft.Bcl.HashCode.dll; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\Microsoft.IO.RecyclableMemoryStream.dll; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\checksum.SHA256; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\bom.json; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags} skipifsourcedoesntexist
Source: {#ReleaseDir}\bom.xml; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags} skipifsourcedoesntexist
Source: {#ReleaseDir}\manifest.spdx.json; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags} skipifsourcedoesntexist
Source: {#ReleaseDir}\manifest.spdx.json.sha256; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags} skipifsourcedoesntexist
Source: {#ReleaseDir}\Twemoji.Mozilla.ttf; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\emojis.xml; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: additional_files\installer.txt; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: additional_files\license.txt; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: additional_files\buckshot-readme.txt; DestName: readme.txt; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
; Writes the start with Windows for the current user, see tasks-icons-reg.iss
Source: additional_files\autostart.inf; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
#ifndef IsLightEdition
; Not in BuckShot Light: it opens files with BuckShot.exe (see tasks-icons-reg.iss)
Source: {#ReleaseDir}\buckshot-proxy.exe; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
Source: {#ReleaseDir}\buckshot-cli.exe; DestDir: {app}; Components: buckshot; Flags: {#DefaultInstallFlags}
#endif

; Core language files
Source: {#LanguagesDir}\*nl-NL*; Excludes: "*installer*,*website*"; DestDir: {app}\Languages; Components: buckshot; Flags: {#DefaultInstallFlags};
Source: {#LanguagesDir}\*en-US*; Excludes: "*installer*,*website*"; DestDir: {app}\Languages; Components: buckshot; Flags: {#DefaultInstallFlags};
Source: {#LanguagesDir}\*de-DE*; Excludes: "*installer*,*website*"; DestDir: {app}\Languages; Components: buckshot; Flags: {#DefaultInstallFlags};

[Components]
Name: "disablesnippingtool"; Description: {cm:disablewin11snippingtool}; Flags: disablenouninstallwarning; Types: default full custom
Name: "buckshot"; Description: "{#AppDisplayName}"; Types: default full compact custom; Flags: fixed

[CustomMessages]
disablewin11snippingtool=Disable Win11 default PrtScr snipping tool
en.disablewin11snippingtool=Disable Win11 default PrtScr snipping tool
de.disablewin11snippingtool=Deaktiviere das Standard Windows 11 Snipping Tool auf "Druck"
ptBR.disablewin11snippingtool=Desativar ferramenta de captura padrão PrtScr do Win11
tr.disablewin11snippingtool=Win11 varsayılan ekran alıntısı aracını devre dışı bırakın
