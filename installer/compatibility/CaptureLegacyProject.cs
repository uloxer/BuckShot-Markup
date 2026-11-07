// Deliberately compiled against the published 0.9.673 assemblies before the source rename.
using System;
using System.Drawing;
using System.IO;
using Greenshot.Base.Core;
using Greenshot.Base.Core.FileFormat;
using Greenshot.Base.Interfaces;
using Greenshot.Editor;
using Greenshot.Editor.Configuration;
using Greenshot.Editor.Drawing;
using Greenshot.Editor.Drawing.Fields;
using Greenshot.Editor.FileFormatHandlers;
class CaptureLegacyProject
{
    [STAThread]
    static void Main(string[] args)
    {
        IniConfigHelper.EnsureInitialized();
        IniConfigHelper.EnsureSection<IEditorConfiguration>(() => new EditorConfigurationImpl());
        var registry = new FileFormatRegistry();
        SimpleServiceProvider.Current.AddService<IFileFormatRegistry>(registry);
        CoreFileFormats.RegisterCoreFileFormats(registry);
        SimpleServiceProvider.Current.AddService<Func<ISurface>>(() => new Surface());
        EditorInitialize.Initialize();
        using (var surface = new Surface(new Bitmap(300, 200)))
        using (var pixels = new Bitmap(12, 8))
        {
            var box = new RectangleContainer(surface) { Left = 20, Top = 30, Width = 60, Height = 40 };
            box.SetFieldValue(FieldType.FILL_COLOR, Color.Aquamarine);
            box.SetFieldValue(FieldType.SHADOW, false);
            surface.AddElement(box, false);
            var polygon = new PolygonContainer(surface);
            polygon.SetVertices(new[] { new Point(100, 30), new Point(150, 30), new Point(100, 90) });
            surface.AddElement(polygon, false);
            surface.SelectElement(box);
            surface.SelectElement(polygon);
            if (!surface.GroupSelectedElements()) throw new Exception("Fixture grouping failed");
            pixels.SetPixel(0, 0, Color.Magenta);
            var image = new ImageContainer(surface) { Image = pixels, Left = 200, Top = 100, Width = 24, Height = 16 };
            surface.AddElement(image, false);
            Directory.CreateDirectory(args[0]);
            using (var stream = File.Create(Path.Combine(args[0], "pre-rename-0.9.673.greenshot")))
                if (!new GreenshotFileFormatHandler().TrySaveToStream((Bitmap)surface.Image, stream, ".greenshot", surface))
                    throw new Exception("Fixture save failed");
            File.WriteAllBytes(Path.Combine(args[0], "pre-rename-0.9.673.clipboard"), DrawableContainerClipboard.Serialize(surface.Elements));
            Console.WriteLine("Captured published 0.9.673 project and clipboard: rectangle, polygon, group, fields, embedded image.");
        }
    }
}
