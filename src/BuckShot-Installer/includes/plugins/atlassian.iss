[Components]
Name: "plugins\atlassian"; Description: {cm:atlassian}; Types: full custom; Flags: disablenouninstallwarning

[Files]
; The plugin and the libraries only it uses; the build removed its copies of the files BuckShot itself installs
Source: {#PluginDir}\BuckShot.Plugin.Atlassian\*.dll; DestDir: {app}\Plugins\BuckShot.Plugin.Atlassian; Components: plugins\atlassian; Flags: {#DefaultInstallFlags};
Source: {#LanguagesDir}\buckshot.atlassian.*.ini; DestDir: {app}\Languages; Components: plugins\atlassian; Flags: {#DefaultInstallFlags};

[CustomMessages]
; Jira and Confluence were two plug-ins up to 1.4
atlassian=Jira and Confluence plug-in
en.atlassian=Jira and Confluence plug-in
de.atlassian=Jira- und Confluence-Plug-in
