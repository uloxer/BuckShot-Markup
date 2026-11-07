#define AutostartComponentGuid "347F4136-4FC7-49F4-8BAF-6157FB408666"
#define AutostartComponentVersion "1,0"
#define AutostartParameters 'advpack.dll,LaunchINFSectionEx ""{app}\autostart.inf"",DefaultInstall,,4'
#define FileOpener "greenshot-proxy.exe"
[Tasks]
Name: startup; Description: "Start BuckShot Markup when I sign in"; Flags: unchecked
[Icons]
Name: {group}\BuckShot Markup; Filename: {app}\Greenshot.exe; WorkingDir: {app}; AppUserModelID: "BuckShot-Markup"
Name: {group}\Uninstall BuckShot Markup; Filename: {uninstallexe}; WorkingDir: {app}
Name: {group}\Licence; Filename: {app}\license.txt; WorkingDir: {app}
[Registry]
Root: HKCU; Subkey: Software\Microsoft\Windows\CurrentVersion\Run; ValueType: string; ValueName: "BuckShot-Markup"; ValueData: """{app}\Greenshot.exe"" --autostart"; Tasks: startup; Flags: uninsdeletevalue
Root: HKA; Subkey: Software\Classes\.greenshot\OpenWithProgids; ValueType: string; ValueName: "BuckShotMarkup.Project"; ValueData: ""; Flags: uninsdeletevalue uninsdeletekeyifempty
Root: HKA; Subkey: Software\Classes\.buckshot\OpenWithProgids; ValueType: string; ValueName: "BuckShotMarkup.Project"; ValueData: ""; Flags: uninsdeletevalue uninsdeletekeyifempty
Root: HKA; Subkey: Software\Classes\BuckShotMarkup.Project; ValueType: string; ValueName: ""; ValueData: "BuckShot Markup project"; Flags: uninsdeletekey
Root: HKA; Subkey: Software\Classes\BuckShotMarkup.Project\DefaultIcon; ValueType: string; ValueData: """{app}\Greenshot.exe"",0"
Root: HKA; Subkey: Software\Classes\BuckShotMarkup.Project\shell\open\command; ValueType: string; ValueData: """{app}\greenshot-proxy.exe"" --file ""%1"""
[Run]
Filename: "{app}\Greenshot.exe"; Description: "Start BuckShot Markup"; Flags: nowait postinstall skipifsilent runasoriginaluser
