[Components]
Name: "plugins\recipeeditor"; Description: {cm:recipeeditor}; Types: default full custom; Flags: disablenouninstallwarning

[Files]
; The plugin and the libraries only it uses; the build removed its copies of the files BuckShot itself installs
Source: {#PluginDir}\BuckShot.Plugin.RecipeEditor\*.dll; DestDir: {app}\Plugins\BuckShot.Plugin.RecipeEditor; Components: plugins\recipeeditor; Flags: {#DefaultInstallFlags};
Source: {#LanguagesDir}\buckshot.recipeeditor.*.ini; DestDir: {app}\Languages; Components: plugins\recipeeditor; Flags: {#DefaultInstallFlags};

[CustomMessages]
recipeeditor=Recipe Editor plug-in
en.recipeeditor=Recipe Editor plug-in
de.recipeeditor=Rezept-Editor Plug-in
ptBR.recipeeditor=Plug-in do Recipe Editor
