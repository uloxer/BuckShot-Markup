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
using BuckShot.Editor.Measurements;

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
        bool expandedOnly = args.Length > 2;
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
                if (!expandedOnly) Capture(editor, args[0], "BuckShot-Store-Screenshot-Measurements.png");
                surface.DeselectAllElements(); surface.SelectElement(line);
                surface.RotateSelectedObjects(-25);
                Application.DoEvents(); editor.Refresh();
                if (!expandedOnly) Capture(editor, args[0], "BuckShot-Store-Screenshot-Rotation.png");
                surface.RotateSelectedObjects(25);
                using (var g = Graphics.FromImage(surface.Image))
                using (var font = new Font("Segoe UI", 15))
                {
                    g.FillRectangle(Brushes.White, 65, 535, 790, 55);
                    g.DrawString("Known reference: bench width = 1000 mm", font, Brushes.SlateGray, 90, 550);
                }
                surface.SetMeasurements(MeasurementState.Calibrate(270, 1000, MeasurementUnit.Millimetres));
                if (!surface.Measurements.IsCalibrated || Math.Abs(surface.Measurements.Convert(270) - 1000) > 0.001)
                    throw new Exception("Real calibration did not convert the known reference correctly.");
                Capture(editor, args[0], "BuckShot-Store-Screenshot-Calibrated.png");
                Console.WriteLine("Calibration metadata: " + surface.Measurements.MetresPerPixel + " metres/pixel; 270 px = " + surface.Measurements.Convert(270) + " mm.");
                surface.Modified = false; editor.Hide();
            }
        }
        CaptureAnnotations(args[0]);
        CaptureGroup(args[0]);
        if (expandedOnly) return;
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
    static void Style(DrawableContainer shape, Color colour, int width)
    {
        shape.SetFieldValue(FieldType.LINE_COLOR, colour);
        shape.SetFieldValue(FieldType.LINE_THICKNESS, width);
        shape.SetFieldValue(FieldType.SHADOW, false);
    }
    static TextContainer Label(Surface surface, string text, int x, int y, int width, Color colour)
    {
        var label = new TextContainer(surface) { Left=x, Top=y, Width=width, Height=48, Text=text };
        Style(label, colour, 0); label.SetFieldValue(FieldType.FONT_SIZE, 17f);
        label.SetFieldValue(FieldType.FILL_COLOR, Color.Transparent);
        surface.AddElement(label, false); return label;
    }
    static ImageEditorForm Open(Surface surface, bool measurements)
    {
        var editor = new ImageEditorForm(surface, true, false);
        editor.StartPosition=FormStartPosition.Manual; editor.Location=new Point(20,20); editor.Size=new Size(1460,960);
        var panel=Field<object>(editor,"_measurementPanel");
        panel.GetType().GetMethod("SetExpanded").Invoke(panel,new object[]{measurements});
        editor.Show(); Application.DoEvents(); surface.ZoomFactor=new Fraction(9,10); return editor;
    }
    static void CaptureAnnotations(string folder)
    {
        using(var image=new Bitmap(900,610))
        {
            using(var g=Graphics.FromImage(image))
            using(var heading=new Font("Segoe UI",26,FontStyle.Bold))
            using(var font=new Font("Segoe UI",18))
            using(var pen=new Pen(Color.LightGray))
            {
                g.Clear(Color.White);g.DrawString("SITE REVIEW CHECKLIST",heading,Brushes.DarkSlateGray,60,45);
                g.DrawString("Sample document for annotation",font,Brushes.SlateGray,60,100);
                g.FillRectangle(Brushes.WhiteSmoke,60,165,770,48);
                g.DrawString("ITEM",font,Brushes.SlateGray,80,175);g.DrawString("STATUS",font,Brushes.SlateGray,570,175);
                string[] items={"Access route","Bench clearance","Lighting position","Storage labels"};
                string[] states={"Confirmed","Check on site","Confirmed","Update drawing"};
                for(int i=0;i<4;i++){int y=230+i*62;g.DrawString(items[i],font,Brushes.DarkSlateGray,80,y);g.DrawString(states[i],font,Brushes.DarkSlateGray,570,y);g.DrawLine(pen,60,y+45,830,y+45);}
                g.DrawString("Sample code: AB12-CD34",font,Brushes.SlateGray,80,525);
            }
            using(var surface=new Surface((Bitmap)image.Clone()))
            using(var editor=Open(surface,false))
            {
                var highlight=new HighlightContainer(surface){Left=565,Top=287,Width=215,Height=40};surface.AddElement(highlight,false);
                var arrow=new ArrowContainer(surface){Left=480,Top=485,Width=-90,Height=-165};Style(arrow,Color.FromArgb(220,100,20),4);surface.AddElement(arrow,false);
                var note=Label(surface,"Check clearance here",390,480,330,Color.FromArgb(180,75,15));
                var box=new RectangleContainer(surface){Left=560,Top=408,Width=245,Height=48};Style(box,Color.FromArgb(0,145,170),3);box.SetFieldValue(FieldType.FILL_COLOR,Color.Transparent);surface.AddElement(box,false);
                var obscure=new ObfuscateContainer(surface){Left=245,Top=520,Width=240,Height=43};surface.AddElement(obscure,false);
                surface.DeselectAllElements();surface.SelectElement(note);
                Capture(editor,folder,"BuckShot-Store-Screenshot-Annotate.png");surface.Modified=false;editor.Hide();
                Console.WriteLine("Annotation: real arrow, text, highlight, rectangle and pixelization containers.");
            }
        }
    }
    static void CaptureGroup(string folder)
    {
        using(var image=new Bitmap(900,610))
        {
            using(var g=Graphics.FromImage(image))
            using(var title=new Font("Segoe UI",24,FontStyle.Bold))
            using(var font=new Font("Segoe UI",16))
            {
                g.Clear(Color.FromArgb(250,252,254));g.DrawString("ARRANGE A VISUAL NOTE",title,Brushes.SlateGray,65,45);
                g.DrawString("Group a shape and label. Snap annotations to object points.",font,Brushes.SlateGray,65,100);
            }
            using(var surface=new Surface((Bitmap)image.Clone()))
            using(var editor=Open(surface,false))
            {
                var box=new RectangleContainer(surface){Left=160,Top=220,Width=260,Height=170};Style(box,Color.FromArgb(0,145,170),4);box.SetFieldValue(FieldType.FILL_COLOR,Color.FromArgb(225,245,248));surface.AddElement(box,false);
                var label=Label(surface,"Assembly bench",175,275,230,Color.DarkSlateGray);
                var target=new RectangleContainer(surface){Left=640,Top=220,Width=140,Height=170};Style(target,Color.SlateGray,3);target.SetFieldValue(FieldType.FILL_COLOR,Color.WhiteSmoke);surface.AddElement(target,false);
                Label(surface,"Storage",625,275,170,Color.SlateGray);
                surface.SelectElement(box);surface.SelectElement(label);
                if(!surface.GroupSelectedElements() || box.GroupId==Guid.Empty || box.GroupId!=label.GroupId)throw new Exception("Group was not created.");
                // Use the normal point/through snapping engine; do not synthesize guide pixels.
                var toolbar=Field<ToolStrip>(editor,"destinationsToolStrip");
                foreach(ToolStripItem item in toolbar.Items){var host=item as ToolStripControlHost;if(host!=null && host.Control is CheckBox)((CheckBox)host.Control).Checked=true;}
                typeof(Surface).GetMethod("SetObjectSnapping",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(surface,new object[]{true,true,12});
                var snapped=(Point)typeof(Surface).GetMethod("SnapGesture",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(surface,new object[]{new Point(820,222),(Point?)new Point(420,220),box,true,null});
                if(Field<object>(surface,"_snapTarget")==null)throw new Exception("Snap engine did not produce a target.");
                surface.Invalidate();Capture(editor,folder,"BuckShot-Store-Screenshot-GroupSnapping.png");
                Console.WriteLine("Grouped shape/label share id; real snap target active, endpoint="+snapped+".");surface.Modified=false;editor.Hide();
            }
        }
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
