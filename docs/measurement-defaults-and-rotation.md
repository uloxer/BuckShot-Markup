# Measurement defaults and object rotation

Settings → Measurements configures the appearance of dimensions in new captures and blank documents: precision, font, size, style, text colour and background. Transparency preserves the chosen background colour. Automatic precision ignores the digits field; significant figures uses at least one digit. Object snapping settings are also available here.

Defaults are copied once when a document is created, without adding an undo step. Saved `.greenshot` projects restore their own document settings, edge overrides and calibration. Calibration is never reused as a default. Use the editor's Measurements panel to change an existing document or an individual dimension.

A single selected supported object has a circular rotation handle outside its transformed upper edge. Drag it to preview rotation; hold Shift for 15° steps. Release commits one undo step. Escape, capture loss, focus loss, a tool change, a selection change or switching document tabs cancels the preview. The handle stays the same screen size as zoom changes and is excluded from exports. The Rotate button still opens the angle dialog. Groups retain their existing dialog rotation behaviour.

Fresh capture settings send the image to both Clipboard and the configured default file destination. PNG/DIB remain the clipboard formats; filenames are not overlaid on the capture. Existing configured destinations, folder, format and filename pattern are preserved. The portable default folder is `Documents\Pictures\BuckShot-Markup`.
