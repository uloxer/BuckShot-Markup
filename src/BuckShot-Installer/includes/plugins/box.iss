[Components]
Name: "plugins\box"; Description: {cm:box}; Types: full custom; Flags: disablenouninstallwarning

[Files]
; The plugin and the libraries only it uses; the build removed its copies of the files BuckShot itself installs
Source: {#PluginDir}\BuckShot.Plugin.Box\*.dll; DestDir: {app}\Plugins\BuckShot.Plugin.Box; Components: plugins\box; Flags: {#DefaultInstallFlags};
Source: {#LanguagesDir}\buckshot.box.*.ini; DestDir: {app}\Languages; Components: plugins\box; Flags: {#DefaultInstallFlags};

[CustomMessages]
box=Box plug-in
en.box=Box plug-in
it.box=Plugin Box
ptBR.box=Plug-in do Box
tr.box=Box eklentisi
