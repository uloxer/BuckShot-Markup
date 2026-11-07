using System;
using System.Drawing;
using System.IO;
using System.Text;
using Svg;

namespace BuckShot.Editor.Forms
{
    /// <summary>Original BuckShot vector artwork, rasterized for the requested device-pixel size.</summary>
    internal static class EditorIconFactory
    {
        internal static Bitmap Create(string resourceName) => Create(resourceName, new Size(64, 64), SystemColors.Menu.GetBrightness() < .5f);

        internal static Bitmap Create(string resourceName, Size size, bool dark)
        {
            string glyph = GlyphFor(resourceName);
            string svg;
            if (glyph.StartsWith("step:", StringComparison.Ordinal))
            {
                string number = glyph.Substring(5);
                svg = "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 24 24\"><rect x=\"2\" y=\"2\" width=\"20\" height=\"20\" rx=\"5\" fill=\"#25d9ed\"/><text x=\"12\" y=\"16.5\" text-anchor=\"middle\" font-family=\"Segoe UI\" font-size=\"12\" font-weight=\"bold\" fill=\"#10243a\">" + number + "</text></svg>";
            }
            else
            {
                using var stream = typeof(EditorIconFactory).Assembly.GetManifestResourceStream("BuckShot.Editor.BuckShotIcons." + glyph + ".svg");
                if (stream == null) throw new InvalidOperationException("Missing BuckShot icon: " + resourceName + " (" + glyph + ")");
                using var reader = new StreamReader(stream, Encoding.UTF8);
                svg = reader.ReadToEnd();
                if (dark) svg = svg.Replace("#10243a", "#f4fbff");
            }
            var document = SvgDocument.FromSvg<SvgDocument>(svg);
            return document.Draw(Math.Max(1, size.Width), Math.Max(1, size.Height));
        }

        internal static string GlyphFor(string resourceName)
        {
            string n = resourceName.Replace(".Image", "");
            if (n.StartsWith("btnStepLabel", StringComparison.Ordinal))
            {
                string value = n.Substring("btnStepLabel".Length);
                return "step:" + (value == "20+" ? "20+" : int.TryParse(value, out int step) ? Math.Max(1, step).ToString() : "1");
            }
            switch (n)
            {
                case "btnCursor": return "cursor";
                case "btnRect": case "addRectangleToolStripMenuItem": return "rectangle";
                case "btnEllipse": case "addEllipseToolStripMenuItem": return "ellipse";
                case "btnLine": case "drawLineToolStripMenuItem": case "arrowHeadNoneMenuItem": return "line";
                case "btnArrow": case "drawArrowToolStripMenuItem": case "arrowHeadsDropDownButton": case "arrowHeadEndMenuItem": return "arrow";
                case "arrowHeadStartMenuItem": return "arrow-start";
                case "arrowHeadBothMenuItem": return "arrow-both";
                case "btnFreehand": case "drawFreehandToolStripMenuItem": return "freehand";
                case "btnText": case "addTextBoxToolStripMenuItem": return "text";
                case "btnSpeechBubble": return "speech";
                case "btnHighlight": case "textHighlightMenuItem": case "areaHighlightMenuItem": case "highlightModeButton": return "highlight";
                case "btnObfuscate": case "obfuscateModeButton": case "pixelizeToolStripMenuItem": return "obfuscate";
                case "toolStripSplitButton1": return "image";
                case "btnResize": return "resize";
                case "btnCrop": return "crop";
                case "rotateCwToolstripButton": return "rotate-cw";
                case "rotateCcwToolstripButton": return "rotate-ccw";
                case "undoToolStripMenuItem": case "btnUndo": return "undo";
                case "redoToolStripMenuItem": case "btnRedo": return "redo";
                case "cutToolStripMenuItem": case "btnCut": return "cut";
                case "copyToolStripMenuItem": case "btnCopy": case "btnClipboard": return "copy";
                case "pasteToolStripMenuItem": case "btnPaste": return "paste";
                case "preferencesToolStripMenuItem": case "btnSettings": return "settings";
                case "removeObjectToolStripMenuItem": case "btnDelete": return "delete";
                case "helpToolStripMenuItem1": case "btnHelp": return "help";
                case "btnSave": return "save";
                case "btnPrint": return "print";
                case "blurToolStripMenuItem": return "blur";
                case "grayscaleHighlightMenuItem": return "grayscale";
                case "magnifyMenuItem": case "zoomInMenuItem": case "zoomStatusDropDownBtn": return "zoom-in";
                case "zoomOutMenuItem": return "zoom-out";
                case "zoomBestFitMenuItem": return "zoom-fit";
                case "zoomActualSizeMenuItem": return "zoom-actual";
                case "btnFillColor": return "fill";
                case "btnLineColor": return "stroke";
                case "fontBoldButton": return "bold";
                case "fontItalicButton": return "italic";
                case "btnAlignTop": return "align-top";
                case "btnAlignMiddle": return "align-middle";
                case "btnAlignBottom": return "align-bottom";
                case "btnAlignLeft": return "align-left";
                case "btnAlignCenter": return "align-center";
                case "btnAlignRight": return "align-right";
                case "shadowButton": return "shadow";
                case "btnConfirm": return "confirm";
                case "btnCancel": case "closeToolStripMenuItem": return "cancel";
                case "AutoCrop": return "autocrop";
                case "CropHorizontal": case "Fit-width": return "crop-horizontal";
                case "CropVertical": case "Fit-height": return "crop-vertical";
                case "PushOut-Top": return "fill-up";
                case "PushOut-Bottom": return "fill-down";
                case "PushOut-Left": return "fill-left";
                case "PushOut-Right": return "fill-right";
                case "Snap-top": return "align-top";
                case "Snap-bottom": return "align-bottom";
                case "Snap-left": return "align-left";
                case "Snap-right": return "align-right";
                case "btnPolygon": return "polygon";
                case "btnGroup": return "group";
                case "btnUngroup": return "ungroup";
                case "btnObjectRotate": return "rotate";
                case "calibrate": return "calibrate";
                case "emoji": return "emoji";
                default: return "image";
            }
        }
    }
}
