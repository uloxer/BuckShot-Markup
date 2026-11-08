# BuckShot Markup Store listing import

The bundle contains a ready-to-import English listing and five genuine captures of the published 0.9.674 editor: pixel dimensions, object rotation, annotation tools, calibrated millimetre measurements, and grouped objects with an active snapping guide. Sample drawings and checklist content are synthetic and contain no customer or private data. Screenshots are rendered by the actual Windows Forms editor; they are not UI mockups. Logos are rendered from the existing BuckShot SVG artwork on white backgrounds.

## Import the complete folder

1. Download `BuckShot-Store-Listing.zip` and extract it.
2. In Partner Center, open BuckShot Markup → Store listings → Import listings → **Import folder**.
3. Select the extracted **BuckShot-Store-Listing** folder. It contains exactly one `listing.csv` and an `images` subfolder. Do not upload the ZIP directly.
4. Review the imported English text, all five screenshots and logo assets. The 300×300 icon is the recommended app tile; the 720×1080 poster is optional.

Image paths include the root folder name, as required by Microsoft's import format. Keep that folder name intact. Field, ID and Type metadata, headers, unused rows and trailer rows are retained from the supplied export. Optional legal fields remain blank; the application includes its required licence notices. The version notes describe 0.9.674; Microsoft's guidance says first-time submissions may leave What's new blank.

## If using a phone

If your browser cannot select a folder, choose **Import .csv** and upload **BuckShot-Store-AndroidImport.csv**. This file contains text only, without local image paths. Then open the English listing and upload the five screenshot PNGs and 300×300 app tile individually. The 720×1080 poster is optional. All seven images are also separate release downloads.

This prepares listing content only. Pricing, availability, age ratings, privacy/support details, package validation, capability declarations and submission approval remain in Partner Center. No URL, email, privacy promise or copyright ownership has been invented. Nothing has been submitted to the Store.

## Reproduce

`CaptureListing.cs` runs against an isolated extraction of the published 0.9.674 MSIX using .NET Framework, its actual BuckShot assemblies and Svg.dll. It uses in-memory design-time configuration, never loads production settings, and does not access the clipboard. Its working directory is a unique temporary folder. It disposes forms without calling their configuration-saving close handler.

Run `build-listing.ps1 -TemplatePath <original exported CSV>` after generating the images. It checks round-trip schema metadata, text limits, image references, PNG sizes and the single-CSV folder rule.

The capture helper verifies actual calibration metadata (270 pixels = 1000 mm), a shared nonempty group ID, and an active target returned by the normal snapping engine. The guide is rendered by the editor. Annotation captures use real arrow, text, highlight, rectangle and obfuscation containers. An optional third command-line argument regenerates only the three expanded showcase shots and preserves the original two captures and logos.

Sources: [Microsoft import/export format](https://learn.microsoft.com/en-us/windows/apps/publish/publish-your-app/msix/import-and-export-store-listings), [listing text requirements](https://learn.microsoft.com/en-us/windows/apps/publish/publish-your-app/msix/add-and-edit-store-listing-info), [screenshot/logo requirements](https://learn.microsoft.com/en-us/windows/apps/publish/publish-your-app/msix/screenshots-and-images).
