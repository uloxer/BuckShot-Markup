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
procedure OpenSourceLink(Sender: TObject; const Link: string; LinkType: TSysLinkType);
var ErrorCode: Integer;
begin
  ShellExecAsOriginalUser('open', 'https://github.com/uloxer/BuckShot-Markup', '', '', SW_SHOWNORMAL, ewNoWait, ErrorCode);
end;
procedure InitializeWizard();
var SourceLink: TNewLinkLabel;
begin
  WizardForm.LicenseMemo.Height := WizardForm.LicenseMemo.Height - ScaleY(28);
  SourceLink := TNewLinkLabel.Create(WizardForm);
  SourceLink.Parent := WizardForm.LicensePage;
  SourceLink.Left := WizardForm.LicenseMemo.Left;
  SourceLink.Top := WizardForm.LicenseMemo.Top + WizardForm.LicenseMemo.Height + ScaleY(4);
  SourceLink.Width := WizardForm.LicenseMemo.Width;
  SourceLink.Caption := '<a href="https://github.com/uloxer/BuckShot-Markup">BuckShot Markup source code on GitHub</a>';
  SourceLink.UseVisualStyle := True;
  SourceLink.OnLinkClick := @OpenSourceLink;
  SourceLink.AdjustHeight();
end;
