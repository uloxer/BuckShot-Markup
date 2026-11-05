param(
    [Parameter(Mandatory = $true)][string] $SvgAssemblyDirectory,
    [string] $OutputDirectory
)
$ErrorActionPreference = 'Stop'
$taskBrandDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not $OutputDirectory) { $OutputDirectory = Join-Path $taskBrandDirectory 'rendered' }
Add-Type -AssemblyName System.Drawing
$taskSvgDirectory = [IO.Path]::GetFullPath($SvgAssemblyDirectory)
$taskResolveHandler = [ResolveEventHandler] {
    param($sender, $eventArgs)
    $taskDependency = Join-Path $taskSvgDirectory (([Reflection.AssemblyName]::new($eventArgs.Name)).Name + '.dll')
    if (Test-Path -LiteralPath $taskDependency) { return [Reflection.Assembly]::LoadFrom($taskDependency) }
    return $null
}.GetNewClosure()
[AppDomain]::CurrentDomain.add_AssemblyResolve($taskResolveHandler)
try {
    [void][Reflection.Assembly]::LoadFrom((Join-Path $taskSvgDirectory 'Svg.dll'))
    [void][IO.Directory]::CreateDirectory($OutputDirectory)
    $taskMark = [Svg.SvgDocument]::Open((Join-Path $taskBrandDirectory 'buckshot-mark.svg'))
    $taskSizes = @(16, 20, 24, 32, 48, 64, 128, 256)
    $taskImages = [Collections.Generic.List[byte[]]]::new()
    foreach ($taskSize in $taskSizes) {
        $taskBitmap = $taskMark.Draw($taskSize, $taskSize)
        $taskStream = [IO.MemoryStream]::new()
        try {
            $taskBitmap.Save((Join-Path $OutputDirectory "buckshot-$taskSize.png"), [Drawing.Imaging.ImageFormat]::Png)
            $taskBitmap.Save($taskStream, [Drawing.Imaging.ImageFormat]::Png)
            $taskImages.Add($taskStream.ToArray())
        } finally { $taskStream.Dispose(); $taskBitmap.Dispose() }
    }
    $taskIconStream = [IO.File]::Create((Join-Path $OutputDirectory 'buckshot.ico'))
    $taskIconWriter = [IO.BinaryWriter]::new($taskIconStream)
    try {
        $taskIconWriter.Write([UInt16]0); $taskIconWriter.Write([UInt16]1); $taskIconWriter.Write([UInt16]$taskSizes.Count)
        $taskOffset = 6 + 16 * $taskSizes.Count
        for ($taskIndex = 0; $taskIndex -lt $taskSizes.Count; $taskIndex++) {
            $taskDimension = if ($taskSizes[$taskIndex] -eq 256) { 0 } else { $taskSizes[$taskIndex] }
            $taskIconWriter.Write([byte]$taskDimension); $taskIconWriter.Write([byte]$taskDimension)
            $taskIconWriter.Write([byte]0); $taskIconWriter.Write([byte]0)
            $taskIconWriter.Write([UInt16]1); $taskIconWriter.Write([UInt16]32)
            $taskIconWriter.Write([UInt32]$taskImages[$taskIndex].Length); $taskIconWriter.Write([UInt32]$taskOffset)
            $taskOffset += $taskImages[$taskIndex].Length
        }
        foreach ($taskImage in $taskImages) { $taskIconWriter.Write([byte[]]$taskImage) }
    } finally { $taskIconWriter.Dispose() }
    $taskBlueprint = [Svg.SvgDocument]::Open((Join-Path $taskBrandDirectory 'blueprint-panel.svg'))
    $taskIllustration = $taskBlueprint.Draw(640, 400)
    try { $taskIllustration.Save((Join-Path $OutputDirectory 'blueprint-panel.png'), [Drawing.Imaging.ImageFormat]::Png) }
    finally { $taskIllustration.Dispose() }

    # Double-resolution wizard artwork remains legible at 200% display scaling.
    $taskWizard = [Drawing.Bitmap]::new(328, 628, [Drawing.Imaging.PixelFormat]::Format24bppRgb)
    $taskGraphics = [Drawing.Graphics]::FromImage($taskWizard)
    $taskLargeMark = $taskMark.Draw(256, 256)
    $taskTitleFont = [Drawing.Font]::new('Segoe UI', 19, [Drawing.FontStyle]::Bold, [Drawing.GraphicsUnit]::Pixel)
    $taskCaptionFont = [Drawing.Font]::new('Segoe UI', 10, [Drawing.FontStyle]::Regular, [Drawing.GraphicsUnit]::Pixel)
    $taskCyan = [Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml('#25d9ed'))
    try {
        $taskGraphics.Clear([Drawing.ColorTranslator]::FromHtml('#10243a'))
        $taskGraphics.ScaleTransform(2, 2)
        $taskGraphics.SmoothingMode = [Drawing.Drawing2D.SmoothingMode]::AntiAlias
        $taskGraphics.TextRenderingHint = [Drawing.Text.TextRenderingHint]::AntiAliasGridFit
        $taskGraphics.DrawImage($taskLargeMark, 20, 25, 124, 124)
        $taskGraphics.DrawString('BuckShot', $taskTitleFont, [Drawing.Brushes]::White, 20, 166)
        $taskGraphics.DrawString('Markup', $taskTitleFont, [Drawing.Brushes]::White, 20, 194)
        $taskGraphics.DrawLine([Drawing.Pens]::DarkSlateGray, 20, 239, 144, 239)
        $taskGraphics.DrawString('CAPTURE', $taskCaptionFont, $taskCyan, 20, 253)
        $taskGraphics.DrawString('MEASURE', $taskCaptionFont, $taskCyan, 20, 270)
        $taskGraphics.DrawString('ANNOTATE', $taskCaptionFont, $taskCyan, 20, 287)
        $taskWizard.Save((Join-Path $OutputDirectory 'installer-large.bmp'), [Drawing.Imaging.ImageFormat]::Bmp)
    } finally {
        $taskGraphics.Dispose(); $taskWizard.Dispose(); $taskLargeMark.Dispose()
        $taskTitleFont.Dispose(); $taskCaptionFont.Dispose(); $taskCyan.Dispose()
    }
    $taskSmall = [Drawing.Bitmap]::new(110, 116, [Drawing.Imaging.PixelFormat]::Format24bppRgb)
    $taskSmallGraphics = [Drawing.Graphics]::FromImage($taskSmall)
    $taskSmallMark = $taskMark.Draw(104, 104)
    try {
        $taskSmallGraphics.Clear([Drawing.Color]::White)
        $taskSmallGraphics.DrawImage($taskSmallMark, 3, 6, 104, 104)
        $taskSmall.Save((Join-Path $OutputDirectory 'installer-small.bmp'), [Drawing.Imaging.ImageFormat]::Bmp)
    } finally { $taskSmallGraphics.Dispose(); $taskSmallMark.Dispose(); $taskSmall.Dispose() }
} finally { [AppDomain]::CurrentDomain.remove_AssemblyResolve($taskResolveHandler) }
