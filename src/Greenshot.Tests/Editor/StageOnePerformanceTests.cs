using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Greenshot.Base.Core;
using Greenshot.Editor.Configuration;
using Greenshot.Editor.Controls;
using Greenshot.Editor.Drawing;
using Greenshot.Editor.Drawing.Fields;
using Greenshot.Editor.FileFormatHandlers;
using Greenshot.Editor.Forms;
using Greenshot.Editor.Measurements;
using Xunit;

namespace Greenshot.Tests.Editor
{
    [Collection(TestCollections.Clipboard)]
    public class StageOnePerformanceTests
    {
        public StageOnePerformanceTests() => TestEnvironment.EnsureInitialized();
        private static T Field<T>(object owner, string name) => (T)owner.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner);
        private static RectangleContainer Box(Surface surface)
        {
            var shape = new RectangleContainer(surface) { Left = 60, Top = 90, Width = 80, Height = 20 };
            surface.AddElement(shape, false); surface.SelectElement(shape); return shape;
        }
        private static void Mouse(Surface surface, string phase, int x, int y) => typeof(Surface)
            .GetMethod("SurfaceMouse" + phase, BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(surface, new object[] { surface, new MouseEventArgs(MouseButtons.Left, 1, x, y, 0) });

        [Fact]
        public void RotationPublishesOneGeometryAndPanelRefreshAndSkipsUnchangedPointer()
        {
            DimensionFormatTests.Sta(() =>
            {
                using var surface = new Surface(new Bitmap(300, 300));
                using var editor = new ImageEditorForm(surface, true, false);
                editor.Show(); Application.DoEvents();
                var shape = Box(surface); shape.SetDimensionVisible(0, true);
                int history = Field<System.Collections.ICollection>(surface, "_undoStack").Count;
                var image = surface.Image;
                Assert.True(surface.TryGetRotationHandle(out var handle));
                int geometry = 0, selection = 0;
                shape.GeometryChanged += (s, e) => geometry++;
                surface.MovingElementChanged += (s, e) => selection++;
                Mouse(surface, "Down", (int)handle.X, (int)handle.Y);
                int refreshes = editor.MeasurementRefreshCount;
                Mouse(surface, "Move", 140, 100);
                Assert.Equal(1, geometry); Assert.Equal(0, selection);
                Assert.Equal(refreshes + 1, editor.MeasurementRefreshCount);
                Mouse(surface, "Move", 140, 100);
                Mouse(surface, "Up", 140, 100);
                Assert.Equal(1, geometry); Assert.Equal(refreshes + 1, editor.MeasurementRefreshCount);
                Assert.Same(image, surface.Image);
                Assert.Equal(history + 1, Field<System.Collections.ICollection>(surface, "_undoStack").Count);
                surface.Undo(); Assert.False(shape.HasObjectTransform);
                Assert.Equal(history, Field<System.Collections.ICollection>(surface, "_undoStack").Count);
                surface.Redo(); Assert.InRange(shape.ObjectAngle, 89, 91);
            });
        }

        [Fact]
        public void RotationAtHighZoomDistinguishesAdjacentDevicePixelsAndCancelsExactly()
        {
            DimensionFormatTests.Sta(() =>
            {
                using var surface = new Surface(new Bitmap(300, 300)); var shape = Box(surface);
                surface.ZoomFactor = new Fraction(8, 1);
                var initial = shape.ObjectTransformState;
                surface.TryGetRotationHandle(out var handle);
                Mouse(surface, "Down", (int)(handle.X * 8), (int)(handle.Y * 8));
                Mouse(surface, "Move", 1120, 800); double first = shape.ObjectAngle;
                Mouse(surface, "Move", 1120, 799);
                Assert.NotEqual(first, shape.ObjectAngle);
                Assert.InRange(Math.Abs(first - shape.ObjectAngle), .01, .3);
                surface.CancelRotationGesture(); Assert.Equal(initial, shape.ObjectTransformState); Assert.False(surface.CanUndo);
                var previousModifiers = InputState.ModifierKeysSource;
                try
                {
                    InputState.ModifierKeysSource = () => Keys.Shift;
                    int changes = 0;
                    shape.GeometryChanged += (s, e) => changes++;
                    Mouse(surface, "Down", (int)(handle.X * 8), (int)(handle.Y * 8));
                    Mouse(surface, "Move", 1120, 800);
                    Mouse(surface, "Move", 1120, 799);
                    Assert.Equal(1, changes);
                    surface.CancelRotationGesture();
                }
                finally { InputState.ModifierKeysSource = previousModifiers; }
            });
        }

        [Fact]
        public void DimensionLayoutReusesImmutableSnapshotAndInvalidatesRelevantInputs()
        {
            using var surface = new Surface(new Bitmap(500, 400));
            var shape = Box(surface); shape.SetDimensionVisible(0, true);
            var unrelated = new RectangleContainer(surface) { Left = 250, Top = 200, Width = 80, Height = 70 };
            surface.AddElement(unrelated, false); unrelated.SetDimensionVisible(0, true);
            var otherLayout = DimensionLayout.Labels(unrelated);
            var layout = DimensionLayout.Labels(shape);
            int builds = shape.DimensionLayoutBuildCount;
            Assert.Same(layout, DimensionLayout.Labels(shape));
            using (var bitmap = new Bitmap(500, 400)) using (var graphics = Graphics.FromImage(bitmap))
            {
                _ = shape.DrawingBounds;
                DimensionLayout.Draw(graphics, shape);
                using var gap = DimensionLayout.ArrowGap(shape);
            }
            Assert.Equal(builds, shape.DimensionLayoutBuildCount);
            Assert.Throws<NotSupportedException>(() => ((IList<DimensionLabel>)layout)[0] = layout[0]);
            void Changed(Action change)
            {
                var before = DimensionLayout.Labels(shape);
                change();
                var after = DimensionLayout.Labels(shape);
                Assert.NotSame(before, after); Assert.Same(after, DimensionLayout.Labels(shape));
            }
            Changed(() => shape.Left++);
            Assert.Same(otherLayout, DimensionLayout.Labels(unrelated));
            Changed(() => shape.RotateObject(25, 100, 100));
            Changed(() => shape.SetFieldValue(FieldType.LINE_THICKNESS, 5));
            Changed(() => shape.SetDimensionFormat(0, new DimensionFormat(DimensionFormatMode.DecimalPlaces, 3)));
            Changed(() => surface.Undo());
            Changed(() => shape.SetDimensionVisible(1, true));
            Changed(() => surface.SetMeasurements(MeasurementState.Calibrate(100, 10, MeasurementUnit.Millimetres)));
            Changed(() => surface.SetDimensionStyle(new DimensionStyle("Arial", 14, FontStyle.Bold, Color.Blue)));
            var originalCulture = System.Globalization.CultureInfo.CurrentCulture;
            try
            {
                var culture = (System.Globalization.CultureInfo)originalCulture.Clone();
                culture.NumberFormat.NumberDecimalSeparator = "!";
                Changed(() => System.Globalization.CultureInfo.CurrentCulture = culture);
            }
            finally { System.Globalization.CultureInfo.CurrentCulture = originalCulture; }
            using var detached = new Surface(new Bitmap(500, 400));
            Changed(() => shape.Parent = detached);
            Changed(() => shape.Parent = surface);
            var arrow = new ArrowContainer(surface) { Left = 20, Top = 200, Width = 300, Height = 0 };
            surface.AddElement(arrow, false); arrow.SetDimensionVisible(0, true);
            var arrowLayout = DimensionLayout.Labels(arrow);
            arrow.SetFieldValue(FieldType.ARROWHEADS, ArrowContainer.ArrowHeadCombination.NONE);
            Assert.NotSame(arrowLayout, DimensionLayout.Labels(arrow));
            var polygon = new PolygonContainer(surface);
            polygon.SetVertices(new[] { new Point(200, 200), new Point(300, 200), new Point(250, 300) });
            surface.AddElement(polygon, false); polygon.SetDimensionVisible(0, true);
            var polygonLayout = DimensionLayout.Labels(polygon);
            polygon.SetVertex(0, new Point(190, 190));
            Assert.NotSame(polygonLayout, DimensionLayout.Labels(polygon));
            var copies = DrawableContainerClipboard.Deserialize(DrawableContainerClipboard.Serialize(new DrawableContainerList { shape }));
            var copied = Assert.IsAssignableFrom<DrawableContainer>(Assert.Single(copies));
            Assert.NotSame(DimensionLayout.Labels(shape), DimensionLayout.Labels(copied));
            copies.Dispose();
            using var bytes = new MemoryStream(); var handler = new GreenshotFileFormatHandler();
            Assert.True(handler.TrySaveToStream((Bitmap)surface.Image, bytes, ".greenshot", surface)); bytes.Position = 0;
            using var loaded = (Surface)handler.LoadSurface(bytes);
            var loadedShape = (DrawableContainer)loaded.Elements[0];
            Assert.NotSame(DimensionLayout.Labels(shape), DimensionLayout.Labels(loadedShape));
            Assert.Same(DimensionLayout.Labels(loadedShape), DimensionLayout.Labels(loadedShape));
        }

        [Fact]
        public void PaletteRetainsImagesUsesToolbarSizeAndDetachesOnRepeatedDisposal()
        {
            DimensionFormatTests.Sta(() =>
            {
                var config = new EditorConfigurationImpl { RecentColors = new List<Color> { Color.Blue, Color.Red } };
                using var toolbar = new ToolStrip();
                var palette = new QuickColorPalette(toolbar, config);
                var target = toolbar.Items.OfType<ToolStripDropDownButton>().Single();
                Assert.True(((ToolStripMenuItem)target.DropDownItems[0]).Checked);
                var images = toolbar.Items.OfType<ToolStripButton>().Where(b => b.Image != null).Select(b => b.Image).ToArray();
                var recents = config.RecentColors;
                palette.Refresh(true, true); palette.Remember(Color.Blue);
                Assert.Same(recents, config.RecentColors);
                Assert.Equal(images, toolbar.Items.OfType<ToolStripButton>().Where(b => b.Image != null).Select(b => b.Image).ToArray());
                var recentSlots = toolbar.Items.OfType<ToolStripButton>().Skip(8).ToArray();
                Assert.Equal(6, recentSlots.Length);
                Assert.Equal(2, recentSlots.Count(button => button.Available));
                foreach (var color in new[] { Color.Green, Color.Gold, Color.Purple, Color.Pink, Color.Brown }) palette.Remember(color);
                Assert.All(recentSlots, button => Assert.True(button.Available));
                Assert.Equal(Color.Brown.ToArgb(), ((Color)recentSlots[0].Tag).ToArgb());
                palette.Refresh(false, false); Assert.False(target.Available);
                Assert.All(recentSlots, button => Assert.False(button.Available));
                palette.Refresh(false, true); Assert.Equal(FieldType.FILL_COLOR, palette.Target);
                Assert.All(recentSlots, button => Assert.True(button.Available));
                palette.Refresh(true, false); Assert.Equal(FieldType.LINE_COLOR, palette.Target);
                toolbar.ImageScalingSize = new Size(32, 32); toolbar.PerformLayout();
                Assert.All(toolbar.Items.OfType<ToolStripButton>().Where(b => b.Image != null), button => Assert.Equal(new Size(32, 32), button.Image.Size));
                palette.Dispose(); palette.Dispose(); Assert.Empty(toolbar.Items.Cast<ToolStripItem>());
                toolbar.ImageScalingSize = new Size(24, 24); toolbar.PerformLayout();
            });
        }

        [Fact]
        public void ColourBarStaysInPropertiesRowAcrossTabsAndOverflowLayouts()
        {
            DimensionFormatTests.Sta(() =>
            {
                using var surface = new Surface(new Bitmap(600, 450));
                using var editor = new ImageEditorForm(surface, true, false) { Size = new Size(1250, 800) };
                var rectangle = Box(surface);
                editor.Show(); Application.DoEvents();
                var first = editor.SelectedTab;
                var toolbar = Field<ToolStrip>(editor, "propertiesToolStrip");
                var palette = Field<QuickColorPalette>(editor, "_quickColors");
                var textSurface = new Surface(new Bitmap(600, 450));
                var text = new TextContainer(textSurface) { Left = 150, Top = 150, Width = 200, Height = 80, Text = "Palette overflow check" };
                textSurface.AddElement(text, false); textSurface.SelectElement(text);
                var second = editor.AddTab(textSurface, true, false);
                string folder = Path.Combine(Environment.GetEnvironmentVariable("GITHUB_WORKSPACE") ?? Path.GetTempPath(), "TestResults", "stage1-colourbar");
                Directory.CreateDirectory(folder);
                foreach (var tab in new[] { first, second })
                foreach (int width in new[] { 1250, 620 })
                {
                    editor.SelectTab(tab); editor.Width = width; Application.DoEvents();
                    var panel = Assert.IsType<ToolStripPanel>(toolbar.Parent);
                    Assert.Equal(2, Array.FindIndex(panel.Rows, row => row.Controls.Contains(toolbar)));
                    Assert.NotNull(palette.Target);
                    foreach (var swatch in toolbar.Items.OfType<ToolStripButton>().Where(button => button.Tag is Color && button.Available))
                        Assert.Equal(toolbar.ImageScalingSize, swatch.Image.Size);
                    using var bitmap = new Bitmap(editor.Width, editor.Height);
                    editor.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
                    bitmap.Save(Path.Combine(folder, (tab == first ? "rectangle" : "text") + "-" + width + ".png"), ImageFormat.Png);
                }
                editor.SelectTab(second);
                editor.Width = 1250;
                foreach (int percent in new[] { 100, 150, 200 })
                {
                    // Simulate toolbar image sizing only; the runner's monitor DPI and fonts are unchanged.
                    int pixels = 16 * percent / 100;
                    toolbar.ImageScalingSize = new Size(pixels, pixels);
                    toolbar.PerformLayout(); Application.DoEvents();
                    foreach (var swatch in toolbar.Items.OfType<ToolStripButton>().Where(button => button.Tag is Color && button.Available))
                        Assert.Equal(new Size(pixels, pixels), swatch.Image.Size);
                    using var bitmap = new Bitmap(editor.Width, editor.Height);
                    editor.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
                    bitmap.Save(Path.Combine(folder, $"text-simulated-swatch-size-{percent}.png"), ImageFormat.Png);
                }
                File.WriteAllText(Path.Combine(folder, "sizing-evidence.txt"),
                    $"100/150/200 percent images simulate 16/24/32 pixel toolbar swatch sizing. Monitor DPI remains {toolbar.DeviceDpi}; fonts and other controls retain that actual DPI.");
                editor.Width = 620; Application.DoEvents();
                Assert.True(toolbar.OverflowButton.Visible);
                toolbar.OverflowButton.ShowDropDown(); Application.DoEvents();
                try
                {
                    var overflow = toolbar.OverflowButton.DropDown;
                    Assert.True(overflow.Visible);
                    Assert.Contains(toolbar.Items.Cast<ToolStripItem>(), item => item.Available && item.Placement == ToolStripItemPlacement.Overflow);
                    using var popup = new Bitmap(overflow.Width, overflow.Height);
                    overflow.DrawToBitmap(popup, new Rectangle(Point.Empty, popup.Size));
                    popup.Save(Path.Combine(folder, "text-open-overflow-dropdown.png"), ImageFormat.Png);
                    using var bitmap = new Bitmap(editor.Width, editor.Height);
                    editor.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
                    bitmap.Save(Path.Combine(folder, "text-open-overflow-editor.png"), ImageFormat.Png);
                }
                finally { toolbar.OverflowButton.HideDropDown(); }
                var target = toolbar.Items.OfType<ToolStripDropDownButton>().Single(button => button.ToolTipText == "Quick colour target");
                ((ToolStripMenuItem)target.DropDownItems[1]).PerformClick();
                editor.SelectTab(first);
                surface.DeselectAllElements();
                var line = new LineContainer(surface) { Left = 200, Top = 200, Width = 120, Height = 40 };
                surface.AddElement(line, false); surface.SelectElement(line);
                Assert.Equal(FieldType.LINE_COLOR, palette.Target);
                Assert.False(target.DropDownItems[1].Enabled);
                editor.SelectTab(second);
                Assert.True(target.DropDownItems[1].Enabled);
            });
        }
    }
}
