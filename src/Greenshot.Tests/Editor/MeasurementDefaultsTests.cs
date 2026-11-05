using System.Drawing;
using System.IO;
using Dapplo.Ini;
using Greenshot.Base.Core;
using Greenshot.Editor.Configuration;
using Greenshot.Editor.Drawing;
using Greenshot.Editor.FileFormatHandlers;
using Greenshot.Editor.Measurements;
using Xunit;

namespace Greenshot.Tests.Editor
{
    [Collection(TestCollections.Clipboard)]
    public class MeasurementDefaultsTests
    {
        public MeasurementDefaultsTests() => TestEnvironment.EnsureInitialized();
        [Fact]
        public void IniValuesRestoreAppearanceAndExplicitCapturePreferencesRemain()
        {
            var config = new EditorConfigurationImpl(); config.ResetToDefaults();
            config.SetRawValue(nameof(config.MeasurementFontFamily), "Consolas");
            config.SetRawValue(nameof(config.MeasurementFontSize), "23");
            config.SetRawValue(nameof(config.MeasurementBackgroundColor), "Gold");
            config.SetRawValue(nameof(config.MeasurementBackgroundTransparent), "False");
            config.SetRawValue(nameof(config.MeasurementPrecisionMode), "DecimalPlaces");
            config.SetRawValue(nameof(config.MeasurementPrecisionDigits), "4");
            var style = MeasurementDefaults.Create(config);
            Assert.Equal("Consolas", style.Family); Assert.Equal(23, style.Size);
            Assert.Equal(Color.Gold.ToArgb(), style.BackgroundColor.ToArgb()); Assert.Equal(4, style.Format.Digits);
            var core = new CoreConfigurationImpl(); core.ResetToDefaults();
            Assert.Equal(new[] { "Clipboard", "FileDefault" }, core.OutputDestinations);
            core.SetRawValue("Destinations", "Editor"); core.OutputFilePath = @"C:\ExistingCaptures";
            core.OutputFileFormat = "jpg"; core.OutputFileFilenamePattern = "my-${title}";
            core.ValidateAndCorrectOutputFilePath();
            Assert.Equal(new[] { "Editor" }, core.OutputDestinations); Assert.Equal(@"C:\ExistingCaptures", core.OutputFilePath);
            Assert.Equal("jpg", core.OutputFileFormat); Assert.Equal("my-${title}", core.OutputFileFilenamePattern);
        }

        [Fact]
        public void MeasurementsSettingsTabRendersWithBoundDefaults()
        {
            DimensionFormatTests.Sta(() =>
            {
                var window = new Greenshot.Settings.Views.SettingsWindow(initialTabName: "measurements");
                try
                {
                    window.Show(); window.UpdateLayout();
                    var tabs = (System.Windows.Controls.TabControl)window.FindName("SettingsTabControl");
                    Assert.Equal("Measurements", ((System.Windows.Controls.TabItem)tabs.SelectedItem).Header);
                    var image = new System.Windows.Media.Imaging.RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
                    image.Render(window);
                    var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder(); encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(image));
                    string folder = Path.Combine(Path.GetTempPath(), "BuckShot-Markup-stage2"); Directory.CreateDirectory(folder);
                    using var output = File.Create(Path.Combine(folder, "measurements-settings.png")); encoder.Save(output);
                }
                finally { window.Close(); }
            });
        }
        [Fact]
        public void FreshDefaultsAreValidatedAndTransparencyRetainsColour()
        {
            var config = new EditorConfigurationImpl
            {
                MeasurementFontFamily = "Arial", MeasurementFontSize = 18, MeasurementFontStyle = FontStyle.Bold,
                MeasurementFontColor = Color.Navy, MeasurementBackgroundColor = Color.Gold,
                MeasurementBackgroundTransparent = true, MeasurementPrecisionMode = DimensionFormatMode.SignificantFigures,
                MeasurementPrecisionDigits = 0
            };
            var style = MeasurementDefaults.Create(config);
            Assert.Equal(1, style.Format.Digits); Assert.Equal(18, style.Size); Assert.Equal(FontStyle.Bold, style.FontStyle);
            Assert.Equal(Color.FromArgb(0, Color.Gold).ToArgb(), style.BackgroundColor.ToArgb());
            config.MeasurementBackgroundTransparent = false;
            Assert.Equal(Color.Gold.ToArgb(), MeasurementDefaults.Create(config).BackgroundColor.ToArgb());
            config.MeasurementPrecisionDigits = 99; config.MeasurementFontSize = float.NaN;
            config.MeasurementFontFamily = ""; config.MeasurementPrecisionMode = (DimensionFormatMode)99;
            Assert.Equal(10, config.MeasurementPrecisionDigits); Assert.Equal(11, config.MeasurementFontSize);
            Assert.Equal("Arial", config.MeasurementFontFamily); Assert.Equal(DimensionFormatMode.Automatic, config.MeasurementPrecisionMode);
        }

        [Fact]
        public void DefaultsApplyOnceWithoutUndoAndProjectStyleOverridesCurrentDefaults()
        {
            var config = IniConfigRegistry.GetSection<IEditorConfiguration>();
            var family = config.MeasurementFontFamily; var size = config.MeasurementFontSize;
            try
            {
                config.MeasurementFontFamily = "Arial"; config.MeasurementFontSize = 19;
                using var fresh = new Surface(new Bitmap(160, 160));
                Assert.Equal(19, fresh.DimensionStyle.Size); Assert.False(fresh.CanUndo); Assert.False(fresh.Measurements.IsCalibrated);
                config.MeasurementFontSize = 27;
                Assert.Equal(19, fresh.DimensionStyle.Size);
                using var file = new MemoryStream(); var handler = new GreenshotFileFormatHandler();
                Assert.True(handler.TrySaveToStream((Bitmap)fresh.Image, file, ".greenshot", fresh)); file.Position = 0;
                using var loaded = (Surface)handler.LoadSurface(file);
                Assert.Equal(19, loaded.DimensionStyle.Size); Assert.False(loaded.CanUndo);
            }
            finally { config.MeasurementFontFamily = family; config.MeasurementFontSize = size; }
        }
    }
}
