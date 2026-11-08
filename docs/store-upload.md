# BuckShot Markup Microsoft Store upload

Download **BuckShot-Markup-Store.msix** and upload that file to the **Packages** section of your BuckShot Markup submission in Partner Center. Use the `.sha256` file to verify the download. The development MSIX, certificate and EXE are separate distribution options; upload the file whose name ends in **-Store.msix**.

This unsigned x64 package uses your reserved identity:

- Name: `ULoXer.BuckShotMarkup`
- Publisher: `CN=39BB6DBD-CEE0-4BD3-A6D6-E8B3D7061493`
- Publisher display name: `ULoXer`
- Package family: `ULoXer.BuckShotMarkup_a54b1ynwgtqsa`
- Version: `0.9.674.0`
- Store ID: `9NF95QP69DZG`

No certificate trust is required for uploading. Microsoft signs packages submitted through the Store; this unsigned upload file is not for direct sideload installation. See [Microsoft's signing overview](https://learn.microsoft.com/en-us/windows/msix/package/signing-package-overview).

The runtime payload is reused from the validated 0.9.674 development MSIX without recompiling the application. CI checks the Store manifest and payload, then signs a separate temporary copy solely to verify the exact Store identity, package family, activation, CLI alias, writable settings and uninstall. That temporary signing certificate and signed copy are not distributed.

Package preparation does not submit the app or complete Store certification. Review Partner Center's package validation, full-trust capability declarations, listing and remaining submission requirements before submitting. After acceptance, the listing URL is https://apps.microsoft.com/detail/9NF95QP69DZG.
