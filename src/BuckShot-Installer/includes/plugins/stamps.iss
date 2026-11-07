[Components]
Name: "plugins\stamps"; Description: {cm:stamps}; Types: full custom; Flags: disablenouninstallwarning

[Files]
; The plugin and the libraries only it uses; the build removed its copies of the files BuckShot itself installs
Source: {#PluginDir}\BuckShot.Plugin.Stamps\*.dll; DestDir: {app}\Plugins\BuckShot.Plugin.Stamps; Components: plugins\stamps; Flags: {#DefaultInstallFlags};
Source: {#LanguagesDir}\buckshot.stamps.*.ini; DestDir: {app}\Languages; Components: plugins\stamps; Flags: {#DefaultInstallFlags};

[CustomMessages]
stamps=Stamps plug-in (beta)
en.stamps=Stamps plug-in (beta)
