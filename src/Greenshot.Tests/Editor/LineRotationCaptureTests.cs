using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Greenshot.Base.Core;
using Greenshot.Base.Interfaces;
using Greenshot.Editor.Configuration;
using Greenshot.Editor.Controls;
using Greenshot.Editor.Drawing;
using Greenshot.Editor.Drawing.Fields;
using Greenshot.Editor.Forms;
using Xunit;

namespace Greenshot.Tests.Editor
{
    [Collection(TestCollections.Clipboard)]
    public class LineRotationCaptureTests
    {
        public LineRotationCaptureTests() => TestEnvironment.EnsureInitialized();
        private static T Field<T>(object owner, string name) => (T)owner.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner);
        private static int UndoCount(Surface surface) => Field<ICollection>(surface, "_undoStack").Count;
        private static void Dispatch(Surface surface, string method, Point imagePoint)
        {
            double zoom = (double)surface.ZoomFactor;
            var device = new Point((int)Math.Round(imagePoint.X * zoom), (int)Math.Round(imagePoint.Y * zoom));
            typeof(Control).GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(surface, new object[] { new MouseEventArgs(MouseButtons.Left, 1, device.X, device.Y, 0) });
        }
        [DllImport("user32.dll", EntryPoint = "SendMessageW")]
        private static extern IntPtr SendMessage(IntPtr window, uint message, IntPtr wParam, IntPtr lParam);

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void CaptureReleasedEditorLineRotationSequence(bool sloped)
        {
            DimensionFormatTests.Sta(() =>
            {
                Assert.Equal("0.9.670", ProductIdentity.Version);
                using var background = new Bitmap(720, 560);
                using (var graphics = Graphics.FromImage(background)) graphics.Clear(Color.White);
                using var surface = new Surface((Bitmap)background.Clone());
                using var editor = new ImageEditorForm(surface, true, false) { Size = new Size(1200, 900) };
                var panel = Field<MeasurementPanel>(editor, "_measurementPanel");
                panel.SetExpanded(true); panel.SetPanelWidth(340);
                editor.Show(); Application.DoEvents();
                surface.ZoomFactor = Fraction.Identity;
                surface.SetObjectSnapping(false, false);
                var line = new LineContainer(surface)
                {
                    Left = 180, Top = sloped ? 340 : 280, Width = 300, Height = sloped ? -140 : 0
                };
                line.SetFieldValue(FieldType.LINE_COLOR, Color.RoyalBlue);
                line.SetFieldValue(FieldType.LINE_THICKNESS, 3);
                line.SetFieldValue(FieldType.SHADOW, false);
                surface.AddElement(line, false); surface.SelectElement(line);
                line.SetDimensionVisible(0, true);
                var edges = Field<DataGridView>(panel, "_dimensionEdges");
                Assert.Single(edges.Rows.Cast<DataGridViewRow>());
                edges.CurrentCell = edges.Rows[0].Cells[1];
                var palette = Field<QuickColorPalette>(editor, "_quickColors");
                var configuration = Field<IEditorConfiguration>(palette, "_configuration");
                var previousColors = configuration.RecentColors?.ToList();
                try
                {
                    palette.Remember(Color.DarkCyan);
                    palette.Remember(Color.DarkOrange);
                    palette.Remember(Color.RoyalBlue);
                    string folder = Path.Combine(Environment.GetEnvironmentVariable("GITHUB_WORKSPACE") ?? Path.GetTempPath(), "TestResults", "line-rotation");
                    Directory.CreateDirectory(folder);
                    string orientation = sloped ? "sloped" : "horizontal";
                    int firstNumber = sloped ? 7 : 1;
                    void Capture(int offset, string phase)
                    {
                        Application.DoEvents(); editor.Update();
                        using var bitmap = new Bitmap(editor.Width, editor.Height);
                        editor.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
                        string name = $"{firstNumber + offset:00}-{orientation}-{phase}";
                        bitmap.Save(Path.Combine(folder, name + ".png"), ImageFormat.Png);
                        var a = line.ModelToImage(new PointF(line.Left, line.Top));
                        var b = line.ModelToImage(new PointF(line.Left + line.Width, line.Top + line.Height));
                        surface.TryGetRotationHandle(out var currentHandle);
                        File.WriteAllText(Path.Combine(folder, name + ".txt"), string.Join(Environment.NewLine, new[]
                        {
                            "Automated rendering of the actual WinForms ImageEditorForm on the Windows CI runner.",
                            "QA-only branch based on released 0.9.670; production code unchanged. Not a physical-desktop screenshot or the local installed app.",
                            "Product version: " + ProductIdentity.Version,
                            "Source commit: " + Environment.GetEnvironmentVariable("GITHUB_SHA"),
                            "Actual control DPI: " + editor.DeviceDpi,
                            "Surface DPI: " + surface.CurrentDpi,
                            "Editor size: " + editor.Size,
                            "Phase: " + phase,
                            "Orientation: " + orientation,
                            "Object rotation degrees: " + line.ObjectAngle.ToString("R", CultureInfo.InvariantCulture),
                            "Image-space endpoints: " + a + " -> " + b,
                            "Rotation handle image position: " + currentHandle,
                            "Dimension visible: " + line.IsDimensionVisible(0),
                            "Rotation gesture active: " + (Field<DrawableContainer>(surface, "_rotationOwner") != null),
                            "Undo entries: " + UndoCount(surface)
                        }));
                    }
                    surface.Focus();
                    Assert.True(surface.TryGetRotationHandle(out var handle));
                    int history = UndoCount(surface);
                    var initial = line.ObjectTransformState;
                    Capture(0, "before-drag");
                    Dispatch(surface, "OnMouseDown", Point.Round(handle));
                    Assert.NotNull(Field<DrawableContainer>(surface, "_rotationOwner"));
                    var pivot = line.ImageCentre;
                    double radians = (sloped ? 35 : 45) * Math.PI / 180;
                    double dx = handle.X - pivot.X, dy = handle.Y - pivot.Y;
                    var end = new Point((int)Math.Round(pivot.X + dx * Math.Cos(radians) - dy * Math.Sin(radians)),
                        (int)Math.Round(pivot.Y + dx * Math.Sin(radians) + dy * Math.Cos(radians)));
                    Dispatch(surface, "OnMouseMove", end);
                    Assert.True(Math.Abs(line.ObjectAngle) > 20);
                    Assert.NotEqual(initial, line.ObjectTransformState);
                    Assert.Equal(history, UndoCount(surface));
                    Capture(1, "mid-drag-before-mouse-up");
                    Assert.NotNull(Field<DrawableContainer>(surface, "_rotationOwner"));
                    Assert.True(surface.Capture);
                    Dispatch(surface, "OnMouseUp", end);
                    Assert.Null(Field<DrawableContainer>(surface, "_rotationOwner"));
                    Assert.Equal(history + 1, UndoCount(surface));
                    Capture(2, "after-release");
                    surface.Undo();
                    Assert.Equal(initial, line.ObjectTransformState);
                    Assert.Equal(history, UndoCount(surface));
                    Capture(3, "after-undo");
                    var cell = edges.GetCellDisplayRectangle(0, 0, false);
                    var glyph = edges.Rows[0].Cells[0].GetContentBounds(0);
                    Assert.True(glyph.Width > 0 && glyph.Height > 0);
                    var position = new IntPtr(((cell.Top + glyph.Top + glyph.Height / 2) << 16) | (cell.Left + glyph.Left + glyph.Width / 2));
                    SendMessage(edges.Handle, 0x0201, new IntPtr(1), position);
                    SendMessage(edges.Handle, 0x0202, IntPtr.Zero, position);
                    Application.DoEvents();
                    Assert.False(line.IsDimensionVisible(0));
                    edges.CurrentCell = edges.Rows[0].Cells[1];
                    Capture(4, "dimension-hidden-by-checkbox");
                    surface.SetObjectSnapping(true, false);
                    surface.DrawingMode = DrawingModes.Line;
                    Dispatch(surface, "OnMouseMove", new Point(line.Left + 2, line.Top));
                    Assert.NotNull(Field<PointF?>(surface, "_snapTarget"));
                    Capture(5, "snapping-guide");
                    surface.ClearSnapGuide();
                }
                finally { configuration.RecentColors = previousColors; }
            });
        }
    }
}
