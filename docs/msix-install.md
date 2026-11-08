# BuckShot Markup MSIX preview

Choose one package format for this product. The EXE installer defaults to a current-user installation and does not require trusting the MSIX development certificate. The MSIX registers for the current Windows user and installs its files in the protected WindowsApps folder. Both contain the same x64 .NET Framework 4.8 desktop app; the MSIX requires Windows 10 version 2004 (19041) or later.

This MSIX uses the development identity `BuckShotMarkup.Preview` and publisher `CN=BuckShot Markup Development`. It is signed with a self-signed test certificate. The non-exportable private key exists only on the disposable build runner and is deleted after signing. Only the public `.cer` is distributed. Each preview build currently creates a new certificate, so a later preview may require trusting its new public certificate too.

## Install the development MSIX

Download `BuckShot-Markup.msix`, its `.sha256`, `BuckShot-Markup-Development.cer` and its `.sha256` from the release. Check the hashes against the release before trusting the certificate:

```powershell
Get-FileHash .\BuckShot-Markup.msix -Algorithm SHA256
Get-FileHash .\BuckShot-Markup-Development.cer -Algorithm SHA256
```

Trusting this development publisher is a separate, one-time administrative action. After reviewing the certificate, run this command in an **Administrator** PowerShell window:

```powershell
Import-Certificate -FilePath .\BuckShot-Markup-Development.cer -CertStoreLocation Cert:\LocalMachine\TrustedPeople
```

Then install for yourself from an ordinary Windows PowerShell window:

```powershell
Add-AppxPackage -Path .\BuckShot-Markup.msix
```

Launch BuckShot Markup from Start. Windows offers it under Open with for `.buckshot` and legacy `.greenshot` projects without replacing an existing default. The packaged startup task is disabled initially; use Windows Settings → Apps → Startup to enable it. The desktop app's legacy registry-based startup option does not control this packaged task.

The packaged command-line entry point is `buckshot-markup-cli.exe` (for example, `buckshot-markup-cli.exe --version`). It uses Windows' app execution alias; do not launch helper EXEs directly from the protected WindowsApps installation folder.

To uninstall for your user:

```powershell
Get-AppxPackage -Name BuckShotMarkup.Preview | Remove-AppxPackage
```

Settings and logs use writable AppData locations, not the read-only package folder. Windows may redirect these paths into the package's per-user data area. Installation does not import or overwrite the existing EXE installation's configuration. Avoid running EXE and MSIX editions simultaneously because they deliberately share the product's single-instance IPC identity. Keep project files outside the package folder.

## Microsoft Store publication

This is a signed development package, not a Store submission. Store distribution requires the reserved package identity, publisher and display name from Partner Center, Store signing and review of the full-trust capability. No Store credentials or private signing key are embedded in the source or downloadable artifacts.

The manual installer workflow builds both formats and tests MSIX signature, current-user registration, package activation, CLI, writable settings and uninstall on a disposable Windows runner. It never installs the package or trusts the certificate on the developer's computer.

References: [Microsoft package signing](https://learn.microsoft.com/en-us/windows/msix/package/create-certificate-package-signing), [desktop manifest extensions](https://learn.microsoft.com/en-us/windows/apps/desktop/modernize/desktop-to-uwp-extensions).
