[Code]
function FullInstall(Param: String): String;
begin Result := SetupMessage(msgFullInstallation); end;
function CompactInstall(Param: String): String;
begin Result := SetupMessage(msgCompactInstallation); end;
function CustomInstall(Param: String): String;
begin Result := SetupMessage(msgCustomInstallation); end;
// Language components use the Windows language-group availability check.
function hasLanguageGroup(argument: String): Boolean;
var KeyValue: String;
begin
  Result := True;
  if RegQueryStringValue(HKLM, 'SYSTEM\CurrentControlSet\Control\Nls\Language Groups', argument, KeyValue) then
    Result := Length(KeyValue) > 0;
end;
procedure InitializeWizard();
begin
  // Accompany the unmodified GPL with source availability inside the scrollable licence page.
  WizardForm.LicenseMemo.Lines.Add('');
  WizardForm.LicenseMemo.Lines.Add('BuckShot Markup corresponding source, including build and installation scripts: https://github.com/uloxer/BuckShot-Markup');
  WizardForm.LicenseMemo.Lines.Add('');
  WizardForm.LicenseMemo.Lines.Add('Copyright and attribution notices');
  WizardForm.LicenseMemo.Lines.Add('Based on Greenshot. Copyright (C) 2007-2026 Thomas Braun, Jens Klingen, Robin Krom and contributors. BuckShot Markup changes by its contributors.');
  WizardForm.LicenseMemo.Lines.Add('Upstream icon credits: Yusuke Kamiyamane and the BuckShot contributors.');
  WizardForm.LicenseMemo.Lines.Add('BuckShot Markup is distributed under the GNU General Public License, version 3 or later, without warranty. The complete licence above and retained component notices apply.');
end;

// Keep the previous Classes defaults across upgrades; never modify Explorer UserChoice.
function AssociationRoot(): Integer;
begin
  if IsAdminInstallMode then Result := HKLM else Result := HKCU;
end;
procedure RegisterProjectAssociation(Extension: String);
var Root: Integer; Previous: String; BackupKey, ExtensionKey: String;
begin
  Root := AssociationRoot();
  BackupKey := 'Software\BuckShot-Markup\Associations\' + Extension;
  ExtensionKey := 'Software\Classes\.' + Extension;
  if not RegKeyExists(Root, BackupKey) then begin
    if RegQueryStringValue(Root, ExtensionKey, '', Previous) then begin
      RegWriteStringValue(Root, BackupKey, 'PreviousDefault', Previous);
      RegWriteDWordValue(Root, BackupKey, 'HadDefault', 1);
    end else RegWriteDWordValue(Root, BackupKey, 'HadDefault', 0);
  end;
  RegWriteStringValue(Root, ExtensionKey, '', 'BuckShotMarkup.Project');
end;
procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then begin
    RegisterProjectAssociation('buckshot');
    RegisterProjectAssociation('greenshot');
  end;
end;
procedure RestoreProjectAssociation(Extension: String);
var Root: Integer; Current, Previous, BackupKey, ExtensionKey: String; HadDefault: Cardinal;
begin
  Root := AssociationRoot();
  BackupKey := 'Software\BuckShot-Markup\Associations\' + Extension;
  ExtensionKey := 'Software\Classes\.' + Extension;
  if RegQueryStringValue(Root, ExtensionKey, '', Current) and (Current = 'BuckShotMarkup.Project') then begin
    if RegQueryDWordValue(Root, BackupKey, 'HadDefault', HadDefault) then begin
      if (HadDefault = 1) and RegQueryStringValue(Root, BackupKey, 'PreviousDefault', Previous) then
        RegWriteStringValue(Root, ExtensionKey, '', Previous)
      else RegDeleteValue(Root, ExtensionKey, '');
    end;
  end;
  RegDeleteKeyIncludingSubkeys(Root, BackupKey);
  RegDeleteKeyIfEmpty(Root, 'Software\BuckShot-Markup\Associations');
  RegDeleteKeyIfEmpty(Root, 'Software\BuckShot-Markup');
end;
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usUninstall then begin
    RestoreProjectAssociation('buckshot');
    RestoreProjectAssociation('greenshot');
  end;
end;
