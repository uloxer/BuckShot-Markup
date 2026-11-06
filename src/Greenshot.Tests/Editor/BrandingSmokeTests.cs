using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Greenshot.Base.Core;
using Greenshot.Editor.Controls;
using Greenshot.Editor.Drawing;
using Greenshot.Editor.Drawing.Fields;
using Greenshot.Editor.Forms;
using Xunit;

namespace Greenshot.Tests.Editor
{
    [Collection(TestCollections.Clipboard)]
    public class BrandingSmokeTests
    {
        public BrandingSmokeTests() => TestEnvironment.EnsureInitialized();

        [Fact]
        public void AboutAndFullEditorRenderCurrentBrandAndRotationHandle()
        {
            DimensionFormatTests.Sta(() =>
            {
                string folder = Path.Combine(Path.GetTempPath(), "BuckShot-Markup-integration"); Directory.CreateDirectory(folder);
                var about = new Greenshot.Views.AboutWindow();
                try
                {
                    about.Show(); about.UpdateLayout();
                    Assert.Equal(ProductIdentity.Name + " " + ProductIdentity.Version, about.AppVersionTitle);
                    AssertNoRepositoryLink(about);
                    var image = FindImage(about);
                    Assert.NotNull(image);
                    Assert.True(((System.Windows.Media.Imaging.BitmapSource)image.Source).PixelWidth > 100);
                    var bitmap = new System.Windows.Media.Imaging.RenderTargetBitmap((int)about.ActualWidth, (int)about.ActualHeight, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
                    bitmap.Render(about);
                    var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder(); encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmap));
                    using var output = File.Create(Path.Combine(folder, "about.png")); encoder.Save(output);
                }
                finally { about.Close(); }

                using var background = new Bitmap(900, 620);
                using (var g = Graphics.FromImage(background)) g.Clear(Color.White);
                using var surface = new Surface((Bitmap)background.Clone());
                using var editor = new ImageEditorForm(surface, true, false) { Size = new Size(1280, 900) };
                var panel = (MeasurementPanel)typeof(ImageEditorForm).GetField("_measurementPanel", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(editor);
                panel.SetExpanded(true); panel.SetPanelWidth(340);
                editor.Show(); Application.DoEvents();
                var tools = (ToolStrip)typeof(ImageEditorForm).GetField("toolsToolStrip", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(editor);
                var cursor = (ToolStripButton)typeof(ImageEditorForm).GetField("btnCursor", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(editor);
                using (var glyph = new Bitmap(cursor.Image))
                {
                    bool lightChrome = tools.BackColor.GetBrightness() > .5f;
                    int contrastingInk = 0;
                    for (int y = 0; y < glyph.Height; y++) for (int x = 0; x < glyph.Width; x++)
                    {
                        var pixel = glyph.GetPixel(x, y);
                        if (pixel.A > 200 && (lightChrome ? pixel.GetBrightness() < .5f : pixel.GetBrightness() > .5f)) contrastingInk++;
                    }
                    Assert.True(contrastingInk > 5, "Enabled glyph must contrast with the actual WinForms toolbar, independently of WPF theme.");
                }
                var box = new RectangleContainer(surface) { Left = 220, Top = 200, Width = 240, Height = 120 };
                box.SetFieldValue(FieldType.FILL_COLOR, Color.FromArgb(220, 241, 250));
                box.SetFieldValue(FieldType.LINE_COLOR, Color.FromArgb(0, 125, 166));
                box.SetFieldValue(FieldType.SHADOW, false);
                surface.AddElement(box, false); surface.SelectElement(box); box.SetDimensionVisible(0, true); box.SetDimensionVisible(1, true);
                Assert.True(surface.RotateSelectedObjects(27)); Assert.True(surface.TryGetRotationHandle(out _));
                surface.Modified = false; Application.DoEvents();
                using var screenshot = new Bitmap(editor.Width, editor.Height);
                editor.DrawToBitmap(screenshot, new Rectangle(Point.Empty, screenshot.Size));
                screenshot.Save(Path.Combine(folder, "editor-toolbar-rotation.png"));
                Assert.Contains(ProductIdentity.Name, editor.Text);
            });
        }

        private static void AssertNoRepositoryLink(System.Windows.DependencyObject parent)
        {
            if (parent is System.Windows.Documents.Hyperlink link)
                Assert.DoesNotContain("github.com", link.NavigateUri?.ToString() ?? string.Empty);
            foreach (object child in System.Windows.LogicalTreeHelper.GetChildren(parent))
                if (child is System.Windows.DependencyObject dependency) AssertNoRepositoryLink(dependency);
        }

        private static System.Windows.Controls.Image FindImage(System.Windows.DependencyObject parent)
        {
            if (parent is System.Windows.Controls.Image image && image.Source is System.Windows.Media.Imaging.BitmapSource source && source.PixelWidth > 100) return image;
            for (int index = 0; index < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); index++)
            {
                var found = FindImage(System.Windows.Media.VisualTreeHelper.GetChild(parent, index));
                if (found != null) return found;
            }
            return null;
        }
    }
}
