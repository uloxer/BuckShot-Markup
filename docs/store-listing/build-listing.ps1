param([Parameter(Mandatory=$true)][string]$TemplatePath)
$ErrorActionPreference='Stop'
$root=$PSScriptRoot
$rows=@(Import-Csv -LiteralPath $TemplatePath)
$headers=@('Field','ID','Type (Type)','default','en-us')
if ((@($rows[0].PSObject.Properties.Name) -join '|') -ne ($headers -join '|')) { throw 'Unexpected listing schema.' }
$copy=@{
 Title='BuckShot Markup'
 ShortTitle='BuckShot Markup'
 ShortDescription='Explain screenshots and drawings with clear annotations, calibrated dimensions and editable visual notes. Highlight details, rotate objects and keep related markup together.'
 DevStudio='ULoXer'
 Description=@'
BuckShot Markup turns screenshots, drawings and images into clear visual instructions. Capture part of your screen, open a file or start from the clipboard, then point out what matters with editable annotations.

Add arrows, text, shapes and highlights to explain a detail or flag an item for review. Blur or pixelate selected areas before sharing. Keep the original context visible so your notes are easy to follow.

Measure layouts and technical details with edge dimensions and object geometry. Set a known reference distance to display measurements in millimetres, centimetres, metres, inches or feet. Calibration belongs to the image, so annotations use a consistent scale.

Rotate objects without losing their editable geometry. Group related shapes and labels, and use point and through snapping to line up your markup. Adjust colours, line styles and dimension labels, with undo and redo as you refine the result.

Save an editable BuckShot project to continue later, or export an annotated image to share in a document, message or report.

Built for Windows desktop workflows: quick screenshot notes, workshop layouts, design reviews and visual instructions.
'@
 ReleaseNotes='Version 0.9.674 improves editor startup, updates BuckShot branding throughout the application and retains support for existing editable projects.'
 Feature1='Capture screenshots or open images from files and the clipboard'
 Feature2='Add arrows, shapes, polygons, text, highlights and blur or pixelation'
 Feature3='Show edge dimensions and inspect object geometry'
 Feature4='Calibrate a known distance for real-world measurements'
 Feature5='Rotate and group annotations, with point and through snapping'
 Feature6='Adjust colours, line styles and dimension labels'
 Feature7='Save editable projects or export annotated images'
 Feature8='Undo and redo changes while refining your markup'
 SearchTerm1='screenshot'
 SearchTerm2='annotation'
 SearchTerm3='image markup'
 SearchTerm4='measurement'
 SearchTerm5='dimensions'
 SearchTerm6='engineering'
 SearchTerm7='screen capture'
 DesktopScreenshotCaption1='Inspect shape geometry and show edge dimensions on an annotated workshop layout.'
 DesktopScreenshotCaption2='Rotate editable annotations while keeping dimension labels and geometry visible.'
 DesktopScreenshotCaption3='Explain a sample checklist with an arrow, text, highlight and pixelated detail.'
 DesktopScreenshotCaption4='Calibrate a known 1000 mm reference and read real-world dimensions across the drawing.'
 DesktopScreenshotCaption5='Keep a shape and label grouped, with real point and through snapping guides for alignment.'
}
$images=@{
 DesktopScreenshot1='BuckShot-Store-Screenshot-Measurements.png'
 DesktopScreenshot2='BuckShot-Store-Screenshot-Rotation.png'
 DesktopScreenshot3='BuckShot-Store-Screenshot-Annotate.png'
 DesktopScreenshot4='BuckShot-Store-Screenshot-Calibrated.png'
 DesktopScreenshot5='BuckShot-Store-Screenshot-GroupSnapping.png'
 StoreLogo300x300='BuckShot-Store-Logo-300.png'
 StoreLogo720x1080='BuckShot-Store-Logo-720x1080.png'
}
foreach($row in $rows){
 if($copy.ContainsKey($row.Field)){$row.'en-us'=$copy[$row.Field]}
 if($row.Field -eq 'Title'){$row.default='BuckShot Markup'}
 if($images.ContainsKey($row.Field)){$row.default='BuckShot-Store-Listing/images/'+$images[$row.Field]}
}
$folder=Join-Path $root 'BuckShot-Store-Listing'
$csv=Join-Path $folder 'listing.csv'
$rows | Export-Csv -LiteralPath $csv -NoTypeInformation -Encoding utf8
$roundtrip=@(Import-Csv -LiteralPath $csv)
$original=@(Import-Csv -LiteralPath $TemplatePath)
if($roundtrip.Count -ne $original.Count){throw 'Listing row count changed.'}
for($i=0;$i -lt $original.Count;$i++){
 foreach($column in @('Field','ID','Type (Type)')){if($roundtrip[$i].$column -cne $original[$i].$column){throw "Schema changed at row $i"}}
}
foreach($row in $roundtrip){
 $text=$row.'en-us';$limit=1000
 if($row.Field -eq 'Description'){$limit=10000}
 elseif($row.Field -eq 'ReleaseNotes'){$limit=1500}
 elseif($row.Field -eq 'ShortTitle'){$limit=50}
 elseif($row.Field -match '^Feature|^DesktopScreenshotCaption'){$limit=200}
 elseif($row.Field -match '^SearchTerm'){$limit=45}
 if($text.Length -gt $limit){throw "Text limit exceeded: $($row.Field)"}
 if($row.default -like 'BuckShot-Store-Listing/*'){
  $path=Join-Path $root $row.default
  if(-not(Test-Path -LiteralPath $path)){throw "Missing image $path"}
 }
}
Add-Type -AssemblyName System.Drawing
foreach($file in Get-ChildItem (Join-Path $folder 'images') -Filter '*.png'){
 $image=[Drawing.Image]::FromFile($file.FullName)
 try{
  if($file.Name -match 'Screenshot' -and ($image.Width -lt 1366 -or $image.Height -lt 768)){throw 'Screenshot below Store minimum.'}
  if($file.Name -eq 'BuckShot-Store-Logo-300.png' -and ($image.Width -ne 300 -or $image.Height -ne 300)){throw 'Wrong tile dimensions.'}
  if($file.Name -eq 'BuckShot-Store-Logo-720x1080.png' -and ($image.Width -ne 720 -or $image.Height -ne 1080)){throw 'Wrong poster dimensions.'}
  if($file.Length -gt 50MB){throw 'Image exceeds Store file limit.'}
  Write-Host "$($file.Name): $($image.Width)x$($image.Height), $($file.Length) bytes"
 }finally{$image.Dispose()}
}
foreach($row in $rows){if($row.'Type (Type)' -like 'Relative path*'){$row.default='';$row.'en-us'=''}}
$rows|Export-Csv -LiteralPath (Join-Path $root 'BuckShot-Store-AndroidImport.csv') -NoTypeInformation -Encoding utf8
if(@(Get-ChildItem -LiteralPath $folder -Filter '*.csv' -Recurse).Count -ne 1){throw 'Import folder must contain exactly one CSV.'}
Write-Host "Verified $($roundtrip.Count) rows; Field/ID/Type metadata unchanged; text limits and $($images.Count) image paths valid."
