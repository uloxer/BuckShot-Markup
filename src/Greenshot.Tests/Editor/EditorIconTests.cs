using System;
using System.Drawing;
using System.Linq;
using Greenshot.Base.Core;
using Greenshot.Editor.Forms;
using Xunit;

namespace Greenshot.Tests.Editor
{
    public class EditorIconTests
    {
        [Theory]
        [InlineData(16)]
        [InlineData(24)]
        [InlineData(32)]
        [InlineData(48)]
        [InlineData(64)]
        public void ToolbarArtworkRendersAtRequestedDeviceSize(int pixels)
        {
            using var image = EditorIconFactory.Create("btnSave.Image", new Size(pixels, pixels), false);
            Assert.Equal(new Size(pixels, pixels), image.Size);
            Assert.True(HasVisiblePixels(image));
        }

        [Fact]
        public void EveryLegacyEditorIconHasAnOriginalVectorReplacement()
        {
            var names = EmbeddedResources.GetNames(typeof(ImageEditorForm)).Where(n => n.EndsWith(".Image", StringComparison.Ordinal)).ToArray();
            Assert.True(names.Length >= 80);
            foreach (var name in names)
            {
                if (name != "toolStripSplitButton1.Image") Assert.NotEqual("image", EditorIconFactory.GlyphFor(name));
                using var image = EditorIconFactory.Create(name, new Size(32, 32), false);
                Assert.True(HasVisiblePixels(image), name);
            }
        }

        [Fact]
        public void DarkToolbarUsesLightArtworkWithoutChangingTransparency()
        {
            using var light = EditorIconFactory.Create("btnSave.Image", new Size(32, 32), false);
            using var dark = EditorIconFactory.Create("btnSave.Image", new Size(32, 32), true);
            bool compared = false;
            for (int y = 0; y < 32; y++)
            for (int x = 0; x < 32; x++)
            {
                var a = light.GetPixel(x, y); var b = dark.GetPixel(x, y);
                Assert.Equal(a.A, b.A);
                if (a.A < 240) continue;
                Assert.True(b.GetBrightness() > a.GetBrightness());
                compared = true;
            }
            Assert.True(compared);
        }

        private static bool HasVisiblePixels(Bitmap image)
        {
            for (int y = 0; y < image.Height; y++)
            for (int x = 0; x < image.Width; x++)
                if (image.GetPixel(x, y).A > 0) return true;
            return false;
        }
    }
}
