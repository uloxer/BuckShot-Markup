using System.Drawing;
using System.IO;
using Greenshot.Base.Interfaces.Drawing;
using Greenshot.Editor.Drawing;
using Greenshot.Editor.Drawing.Fields;
using Greenshot.Editor.FileFormatHandlers;
using Greenshot.Editor.Memento;
using Xunit;

namespace Greenshot.Tests.Editor
{
    [Collection(TestCollections.Clipboard)]
    public class ImageHistoryOwnershipTests
    {
        public ImageHistoryOwnershipTests() => TestEnvironment.EnsureInitialized();

        private static ImageContainer Paste(Surface surface)
        {
            using var bitmap = new Bitmap(20, 20);
            using (var graphics = Graphics.FromImage(bitmap)) graphics.Clear(Color.Magenta);
            var image = new ImageContainer(surface) { Image = bitmap, Left = 10, Top = 10, Width = 20, Height = 20 };
            surface.AddElements(new DrawableContainerList { image });
            surface.SelectElement(image);
            return image;
        }

        private static void Visible(Surface surface, ImageContainer image)
        {
            Assert.Contains(image, surface.Elements); Assert.NotNull(image.Image);
            using var rendered = (Bitmap)surface.GetImageForExport();
            Assert.Equal(Color.Magenta.ToArgb(), rendered.GetPixel(20, 20).ToArgb());
        }

        [Fact]
        public void DeleteUndoThenNewEditPreservesPastedPixelsAndProject()
        {
            using var surface = new Surface(new Bitmap(100, 100)); var image = Paste(surface);
            surface.RemoveSelectedElements(); Assert.Empty(surface.Elements);
            surface.Undo(); Visible(surface, image);
            surface.AddElement(new RectangleContainer(surface) { Left = 60, Top = 60, Width = 10, Height = 10 });
            Visible(surface, image); surface.Undo(); Visible(surface, image); surface.Redo(); Visible(surface, image);
            using var bytes = new MemoryStream(); var format = new GreenshotFileFormatHandler();
            Assert.True(format.TrySaveToStream((Bitmap)surface.Image, bytes, ".greenshot", surface)); bytes.Position = 0;
            using var loaded = (Surface)format.LoadSurface(bytes);
            using var rendered = (Bitmap)loaded.GetImageForExport();
            Assert.Equal(Color.Magenta.ToArgb(), rendered.GetPixel(20, 20).ToArgb());
        }

        [Theory]
        [InlineData("add")]
        [InlineData("bounds")]
        [InlineData("field")]
        public void DiscardedEditHistoryDoesNotOwnAttachedImage(string kind)
        {
            using var surface = new Surface(new Bitmap(100, 100)); var image = Paste(surface);
            // Discarding an old add record is also what history pruning does.
            IMemento history = kind == "add" ? new AddElementsMemento(surface, new DrawableContainerList { image }) :
                kind == "bounds" ? new DrawableContainerBoundsChangeMemento(image) : new ChangeFieldHolderMemento(image, image.GetField(FieldType.SHADOW));
            history.Dispose();
            Visible(surface, image);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void RestoredDeleteRecordRelinquishesImageOwnership(bool single)
        {
            using var surface = new Surface(new Bitmap(100, 100)); var image = Paste(surface);
            IMemento deletion = single ? new DeleteElementMemento(surface, image) : new DeleteElementsMemento(surface, new DrawableContainerList { image });
            surface.RemoveElements(new DrawableContainerList { image }, false);
            using var restored = deletion.Restore(); deletion.Dispose();
            Visible(surface, image);
        }

        [Fact]
        public void DiscardedRedoOfUndonePasteDisposesDetachedImage()
        {
            using var surface = new Surface(new Bitmap(100, 100)); var image = Paste(surface);
            surface.Undo(); Assert.Empty(surface.Elements); Assert.NotNull(image.Image);
            surface.AddElement(new RectangleContainer(surface) { Left = 60, Top = 60, Width = 10, Height = 10 });
            Assert.Null(image.Image); Assert.False(surface.CanRedo);
        }
    }
}
