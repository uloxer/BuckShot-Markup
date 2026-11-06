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
  WizardForm.LicenseMemo.Lines.Add('Upstream icon credits: Yusuke Kamiyamane and the Greenshot contributors.');
  WizardForm.LicenseMemo.Lines.Add('BuckShot Markup is distributed under the GNU General Public License, version 3 or later, without warranty. The complete licence above and retained component notices apply.');
end;
