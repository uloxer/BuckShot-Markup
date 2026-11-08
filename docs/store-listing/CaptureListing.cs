// Renders real published editor controls against a synthetic, non-private sample drawing.
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using BuckShot.Base.Core;
using BuckShot.Base.Core.FileFormat;
using BuckShot.Base.Interfaces;
using BuckShot.Base.Languages;
using BuckShot.Editor;
using BuckShot.Editor.Configuration;
using BuckShot.Editor.Controls;
using BuckShot.Editor.Drawing;
using BuckShot.Editor.Drawing.Fields;
using BuckShot.Editor.Forms;

class CaptureListing
{
    static void Dimension(DrawableContainer shape, int edge)
    {
        typeof(DrawableContainer).GetMethod("SetDimensionVisible", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(shape, new object[] { edge, true });
    }
    static T Field<T>(object owner, string name) { return (T)owner.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner); }
    [STAThread] static void Main(string[] args)
    {
        Directory.CreateDirectory(args[0]);
        Application.EnableVisualStyles();
        IniConfigHelper.EnsureInitialized();
        IniConfigHelper.EnsureSection<IEditorConfiguration>(() => new EditorConfigurationImpl());
        var registry = new FileFormatRegistry();
        SimpleServiceProvider.Current.AddService<IFileFormatRegistry>(registry);
        CoreFileFormats.RegisterCoreFileFormats(registry);
        EditorInitialize.Initialize();
        Texts.Initialize(new[] { Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Languages") });
        Texts.SystemLanguage = "en-US"; Texts.SetLanguage("en-US");
        using (var drawing = new Bitmap(900, 610))
        {
            using (var g = Graphics.FromImage(drawing))
            using (var grid = new Pen(Color.FromArgb(232, 237, 242)))
            using (var wall = new Pen(Color.FromArgb(75, 94, 113), 5))
            using (var small = new Font("Segoe UI", 15))
            using (var title = new Font("Segoe UI", 23, FontStyle.Bold))
            {
                g.Clear(Color.FromArgb(250, 252, 254));
                for (int x = 20; x < 900; x += 25) g.DrawLine(grid, x, 20, x, 590);
                for (int y = 20; y < 610; y += 25) g.DrawLine(grid, 20, y, 880, y);
                g.DrawString("WORKSHOP LAYOUT", title, Brushes.SlateGray, 65, 35);
                g.DrawRectangle(wall, 90, 150, 680, 365);
                g.DrawLine(wall, 500, 150, 500, 340);
                g.DrawLine(wall, 500, 420, 500, 515);
                g.FillRectangle(Brushes.WhiteSmoke, 145, 220, 270, 140);
                g.DrawRectangle(Pens.SlateGray, 145, 220, 270, 140);
                g.DrawString("ASSEMBLY BENCH", small, Brushes.SlateGray, 180, 272);
                g.DrawString("STORAGE", small, Brushes.SlateGray, 580, 240);
                g.DrawString("ACCESS", small, Brushes.SlateGray, 555, 435);
                g.DrawArc(Pens.SlateGray, 420, 340, 160, 160, 270, 90);
                g.DrawString("Sample drawing  |  dimensions shown in pixels", small, Brushes.SlateGray, 90, 550);
            }
            using (var surface = new Surface((Bitmap)drawing.Clone()))
            using (var editor = new ImageEditorForm(surface, true, false))
            {
                editor.StartPosition = FormStartPosition.Manual;
                editor.Location = new Point(20, 20); editor.Size = new Size(1460, 960);
                var panel = Field<object>(editor, "_measurementPanel");
                panel.GetType().GetMethod("SetExpanded").Invoke(panel, new object[] { true });
                panel.GetType().GetMethod("SetPanelWidth", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(panel, new object[] { 340 });
                var boundary = new RectangleContainer(surface) { Left = 90, Top = 150, Width = 680, Height = 365 };
                boundary.SetFieldValue(FieldType.LINE_COLOR, Color.FromArgb(0, 145, 170));
                boundary.SetFieldValue(FieldType.LINE_THICKNESS, 3);
                boundary.SetFieldValue(FieldType.FILL_COLOR, Color.Transparent);
                boundary.SetFieldValue(FieldType.SHADOW, false);
                surface.AddElement(boundary, false); Dimension(boundary, 0); Dimension(boundary, 1);
                var line = new LineContainer(surface) { Left = 145, Top = 395, Width = 270, Height = 0 };
                line.SetFieldValue(FieldType.LINE_COLOR, Color.FromArgb(210, 115, 20));
                line.SetFieldValue(FieldType.LINE_THICKNESS, 3); line.SetFieldValue(FieldType.SHADOW, false);
                surface.AddElement(line, false); Dimension(line, 0);
                surface.SelectElement(boundary);
                editor.Show(); Application.DoEvents(); surface.ZoomFactor = new Fraction(9, 10); editor.Refresh();
                Capture(editor, args[0], "BuckShot-Store-Screenshot-Measurements.png");
                surface.DeselectAllElements(); surface.SelectElement(line);
                surface.RotateSelectedObjects(-25);
                Application.DoEvents(); editor.Refresh();
                Capture(editor, args[0], "BuckShot-Store-Screenshot-Rotation.png");
                surface.Modified = false; editor.Hide();
            }
        }
        var mark = Svg.SvgDocument.Open(Path.Combine(args[1], "buckshot-mark.svg"));
        using (var tile = new Bitmap(300, 300))
        using (var g = Graphics.FromImage(tile))
        using (var icon = mark.Draw(270, 270))
        {
            g.Clear(Color.White); g.DrawImage(icon, 15, 15);
            tile.Save(Path.Combine(args[0], "BuckShot-Store-Logo-300.png"), ImageFormat.Png);
        }
        using (var poster = new Bitmap(720, 1080))
        using (var g = Graphics.FromImage(poster))
        using (var icon = mark.Draw(480, 480))
        using (var title = new Font("Segoe UI", 56, FontStyle.Bold, GraphicsUnit.Pixel))
        using (var subtitle = new Font("Segoe UI", 30, FontStyle.Regular, GraphicsUnit.Pixel))
        {
            g.Clear(Color.White); g.DrawImage(icon, 120, 80);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            var format = new StringFormat { Alignment = StringAlignment.Center };
            g.DrawString("BuckShot Markup", title, Brushes.MidnightBlue, new RectangleF(20, 575, 680, 80), format);
            g.DrawString("Capture. Measure. Annotate.", subtitle, Brushes.DarkSlateGray, new RectangleF(20, 660, 680, 50), format);
            poster.Save(Path.Combine(args[0], "BuckShot-Store-Logo-720x1080.png"), ImageFormat.Png);
        }
        Console.WriteLine("Captured real editor images and vector-derived logos.");
    }
    static void Capture(Form editor, string folder, string name)
    {
        Application.DoEvents(); editor.Update();
        using (var image = new Bitmap(editor.Width, editor.Height))
        {
            editor.DrawToBitmap(image, new Rectangle(Point.Empty, image.Size));
            image.Save(Path.Combine(folder, name), ImageFormat.Png);
            Console.WriteLine(name + " " + image.Width + "x" + image.Height);
        }
    }
}
