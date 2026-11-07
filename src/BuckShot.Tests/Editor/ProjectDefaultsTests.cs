using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BuckShot.Base.Controls;
using BuckShot.Base.Core;
using BuckShot.Base.Core.Export;
using BuckShot.Base.Core.FileFormat;
using BuckShot.Base.Interfaces;
using BuckShot.Base.Interfaces.Plugin;
using BuckShot.Base.Threading;
using BuckShot.Destinations;
using BuckShot.Editor.Configuration;
using BuckShot.Editor.Controls;
using BuckShot.Editor.Drawing;
using BuckShot.Editor.Drawing.Fields;
using BuckShot.Editor.FileFormatHandlers;
using BuckShot.Editor.Forms;
using BuckShot.Editor.Measurements;
using Xunit;

namespace BuckShot.Tests.Editor
{
    [Collection(TestCollections.Clipboard)]
    public class ProjectDefaultsTests
    {
        public ProjectDefaultsTests() => TestEnvironment.EnsureInitialized();
        private static T Field<T>(object owner, string name) => (T)owner.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner);

        [Fact]
        public void FreshDefaultsAndLoadedChoicesRemainDistinct()
        {
            var core = new CoreConfigurationImpl(); core.ResetToDefaults();
            var editor = new EditorConfigurationImpl(); editor.ResetToDefaults();
            Assert.Equal("Ctrl + Shift + S", core.RegionHotkey);
            Assert.Equal("Ctrl + Shift + C", core.ClipboardHotkey);
            Assert.Equal(new[] { new FileWithDialogDestination().Designation, new ClipboardDestination().Designation }, core.OutputDestinations);
            Assert.Equal(WellKnownFileFormats.Buckshot, core.OutputFileFormat);
            var style = MeasurementDefaults.Create(editor);
            Assert.Equal(DimensionFormatMode.DecimalPlaces, style.Format.Mode);
            Assert.Equal(1, style.Format.Digits); Assert.Equal(Color.White.ToArgb(), style.BackgroundColor.ToArgb());
            Assert.Equal(DimensionFormatMode.DecimalPlaces, DimensionStyle.Default.Format.Mode);
            Assert.Equal(1, DimensionStyle.Default.Format.Digits); Assert.Equal(255, DimensionStyle.Default.BackgroundColor.A);
            core.SetRawValue(nameof(core.RegionHotkey), "Alt + F9");
            core.SetRawValue(nameof(core.ClipboardHotkey), "Alt + F10");
            core.SetRawValue(nameof(core.OutputFileFormat), "png");
            core.SetRawValue("Destinations", "Editor,Clipboard");
            editor.SetRawValue(nameof(editor.MeasurementPrecisionMode), "SignificantFigures");
            editor.SetRawValue(nameof(editor.MeasurementPrecisionDigits), "4");
            editor.SetRawValue(nameof(editor.MeasurementBackgroundTransparent), "True");
            Assert.Equal("Alt + F9", core.RegionHotkey); Assert.Equal("Alt + F10", core.ClipboardHotkey);
            Assert.Equal("png", core.OutputFileFormat); Assert.Equal(new[] { "Editor", "Clipboard" }, core.OutputDestinations);
            Assert.Equal(DimensionFormatMode.SignificantFigures, MeasurementDefaults.Create(editor).Format.Mode);
            Assert.Equal(4, MeasurementDefaults.Create(editor).Format.Digits); Assert.Equal(0, MeasurementDefaults.Create(editor).BackgroundColor.A);
            Assert.Equal("Ctrl+Alt+S", new FileWithDialogDestination().Descriptor.Shortcut);
            Assert.Equal("Ctrl+Alt+C", new ClipboardDestination().Descriptor.Shortcut);
        }

        [Theory]
        [InlineData("buckshot")]
        [InlineData("greenshot")]
        public void ProjectExtensionsRoundTripEditableObjectsImagesAndMetadata(string format)
        {
            DimensionFormatTests.Sta(() =>
            {
                using var source = new Surface(new Bitmap(500, 400));
                var shape = new RectangleContainer(source) { Left = 40, Top = 60, Width = 140, Height = 100 };
                source.AddElement(shape, false); shape.SetDimensionVisible(0, true);
                shape.SetDimensionFormat(0, new DimensionFormat(DimensionFormatMode.DecimalPlaces, 3));
                source.SelectElement(shape); source.RotateSelectedObjects(27);
                using var pixels = new Bitmap(20, 10); pixels.SetPixel(0, 0, Color.Magenta);
                var image = new ImageContainer(source) { Image = pixels, Left = 230, Top = 180, Width = 80, Height = 40 };
                source.AddElement(image, false);
                source.SetMeasurements(MeasurementState.Calibrate(100, 2, MeasurementUnit.Metres));
                source.SetDimensionStyle(new DimensionStyle("Arial", 13, FontStyle.Bold, Color.Navy, DimensionFormat.Automatic, Color.Transparent));
                var registry = SimpleServiceProvider.Current.GetInstance<IFileFormatRegistry>();
                Assert.Equal(format, registry.GetByExtension("." + format.ToUpperInvariant()).Id);
                Assert.Equal(format, ImageIO.FormatForFilename("test." + format.ToUpperInvariant()));
                Assert.Contains(registry.GetSaveableFileFormats(), f => f.Id == format);
                Assert.Contains(registry.GetLoadableFileFormats(), f => f.Id == format);
                Assert.False(WellKnownFileFormats.IsEqualFormat(WellKnownFileFormats.Buckshot, WellKnownFileFormats.LegacyProject));
                Assert.True(WellKnownFileFormats.IsProjectFormat(format.ToUpperInvariant()));
                Assert.False(ImageIO.CreateImageFromSurface(source, new SurfaceOutputSettings(format), out var original));
                Assert.Same(source.Image, original);
                using var stream = new MemoryStream();
                ImageIO.SaveToStream(source, stream, new SurfaceOutputSettings(format));
                stream.Position = 0;
                using var loaded = (Surface)new BuckShotFileFormatHandler().LoadSurface(stream);
                Assert.Equal(2, loaded.Elements.Count);
                var restored = Assert.IsType<RectangleContainer>(loaded.Elements[0]);
                Assert.Equal(shape.ObjectTransformState, restored.ObjectTransformState);
                Assert.True(restored.IsDimensionVisible(0)); Assert.Equal(3, restored.GetDimensionFormat(0).Digits);
                Assert.Equal(source.Measurements.Unit, loaded.Measurements.Unit); Assert.Equal(source.Measurements.MetresPerPixel, loaded.Measurements.MetresPerPixel); Assert.Equal(source.DimensionStyle, loaded.DimensionStyle);
                Assert.Equal(Color.Magenta.ToArgb(), ((Bitmap)Assert.IsType<ImageContainer>(loaded.Elements[1]).Image).GetPixel(0, 0).ToArgb());
            });
        }

        [Fact]
        public void SaveDialogKeepsTypedProjectExtensionsAndClipboardRemainsPasteable()
        {
            DimensionFormatTests.Sta(() =>
            {
                using var surface = new Surface(new Bitmap(80, 60));
                var dialog = new SaveImageFileDialog(surface.CaptureDetails);
                dialog.Extension = ".buckshot";
                dialog.FileName = "editable.BUCKSHOT"; Assert.Equal("editable.BUCKSHOT", dialog.FileNameWithExtension);
                dialog.FileName = "legacy.BUCKSHOT"; Assert.Equal("legacy.BUCKSHOT", dialog.FileNameWithExtension);
                dialog.FileName = "new-project"; Assert.Equal("new-project.buckshot", dialog.FileNameWithExtension);
                var box = new RectangleContainer(surface) { Left = 10, Top = 10, Width = 40, Height = 30 };
                box.SetFieldValue(FieldType.FILL_COLOR, Color.Red); surface.AddElement(box, false);
                using var source = new SurfaceExportSource(surface, InlineUiDispatcher.Instance, surface.Modified);
                var request = new ExportRequest(source, surface.CaptureDetails, true, HeadlessUserInteraction.Instance);
                new ClipboardDestination().ExportAsync(request, CancellationToken.None).GetAwaiter().GetResult();
                using var pasted = ClipboardService.Current.GetImageAsync().GetAwaiter().GetResult();
                Assert.NotNull(pasted); Assert.Equal(surface.Image.Size, pasted.Size);
                Assert.Equal(Color.Red.ToArgb(), ((Bitmap)pasted).GetPixel(25, 25).ToArgb());
            });
        }

        [Theory]
        [InlineData(1f, 280)]
        [InlineData(1.5f, 340)]
        [InlineData(2f, 280)]
        public void LargeFontAndIconLayoutKeepsPreferredRowHeights(float scale, int panelWidth)
        {
            DimensionFormatTests.Sta(() =>
            {
                using var surface = new Surface(new Bitmap(700, 600));
                using var editor = new ImageEditorForm(surface, true, false) { Size = new Size(1200, 1000) };
                var panel = Field<MeasurementPanel>(editor, "_measurementPanel");
                panel.SetExpanded(true); panel.SetPanelWidth(panelWidth);
                foreach (var name in new[] { "propertiesToolStrip", "toolsToolStrip", "destinationsToolStrip" })
                    Field<ToolStrip>(editor, name).ImageScalingSize = new Size((int)(16 * scale), (int)(16 * scale));
                using var font = new Font(panel.Font.FontFamily, panel.Font.Size * scale);
                panel.Font = font;
                var line = new LineContainer(surface) { Left = 100, Top = 200, Width = 300, Height = 80 };
                surface.AddElement(line, false); surface.SelectElement(line); line.SetDimensionVisible(0, true);
                surface.SetDimensionStyle(new DimensionStyle("Arial Narrow", 144, FontStyle.Bold, Color.Black, DimensionStyle.Default.Format, Color.White));
                editor.Show(); Application.DoEvents(); panel.PerformLayout(); Application.DoEvents();
                foreach (var name in new[] { "_properties", "_dimensionEdges" })
                {
                    var grid = Field<DataGridView>(panel, name);
                    Assert.Equal(DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders, grid.AutoSizeRowsMode);
                    foreach (DataGridViewRow row in grid.Rows)
                        foreach (DataGridViewCell cell in row.Cells)
                            if (!(cell is DataGridViewCheckBoxCell))
                                Assert.True(row.Height >= TextRenderer.MeasureText("Ag", cell.InheritedStyle.Font).Height + cell.InheritedStyle.Padding.Vertical, $"{name} row {row.Index} clipped at {scale}");
                }
                foreach (var name in new[] { "_documentFormat", "_edgeFormat", "_documentDigits", "_edgeDigits" })
                {
                    var control = Field<Control>(panel, name);
                    Assert.True(control.Height >= control.PreferredSize.Height, $"{name} clipped at {scale}");
                    Assert.True(control.Parent.Height >= control.Bottom, $"{name} parent clipped at {scale}");
                }
                string directory = Path.Combine(Environment.GetEnvironmentVariable("GITHUB_WORKSPACE") ?? Path.GetTempPath(), "TestResults", "project-defaults"); Directory.CreateDirectory(directory);
                using var bitmap = new Bitmap(editor.Width, editor.Height); editor.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
                bitmap.Save(Path.Combine(directory, $"measurement-layout-{scale:0.0}-{panelWidth}-top.png"), ImageFormat.Png);
                Field<Panel>(panel, "_body").ScrollControlIntoView(Field<Label>(panel, "_dimensionStyleLabel")); Application.DoEvents();
                editor.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
                bitmap.Save(Path.Combine(directory, $"measurement-layout-{scale:0.0}-{panelWidth}-font.png"), ImageFormat.Png);
                File.WriteAllText(Path.Combine(directory, $"measurement-layout-{scale:0.0}-{panelWidth}.txt"), $"Actual DeviceDpi={editor.DeviceDpi}; simulated font and toolbar icon scale={scale}; version={typeof(Surface).Assembly.GetName().Version}");
            });
        }
    }
}
