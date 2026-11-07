# BuckShot Markup

Windows screenshot capture and engineering markup, based on Greenshot and GreenShotPlus.

Current product version: **0.9.671**. `src/version.json` is the authoritative version source.

The EXE installer is the default Windows package. MSIX builds are optional and disabled by default.

0.8.667 fixes pasted images disappearing after undoing a deletion, movement or style edit and then making another change. Discarded history preserves live objects while still releasing detached deleted images.

- Capture a region, window or screen and annotate images with editable shapes, text, polygons, calibrated measurements and dimensions.
- Use resizable measurement controls, independent dimension backgrounds, object groups and rotation.
- Drag the circular rotation handle on a selected individual object, or use Rotate for an exact angle. Shift snaps the drag to 15 degrees; Escape cancels it.
- Choose new-document precision, font, colours, transparency and snapping under Settings > Measurements. Saved projects keep their own settings and calibration.
- Fresh installations copy captures to the clipboard and save directly using the preferred output folder, file name and format. Existing explicit preferences are preserved.
- Public recipe authoring/import/catalog features are not part of BuckShot Markup. The private built-in capture pipeline remains.
- Settings and Windows integration are isolated from an existing Greenshot/GreenShotPlus installation.

See [migration and compatibility audit](docs/buckshot-migration-audit.md) and [editor usage](docs/greenshotplus.md).

![BuckShot Markup cyan blueprint identity](assets/branding/rendered/buckshot-128.png)

The [brand review](assets/branding/review/cyan-blueprint-direction.png) and [original scalable toolbar artwork](assets/branding/toolbar) are included in the source.

Based on Greenshot, copyright © 2007–2026 Thomas Braun, Jens Klingen, Robin Krom and contributors. Existing licences, copyrights and credits are retained. Distributed under the [GNU GPL version 3 or later](LICENSE).
