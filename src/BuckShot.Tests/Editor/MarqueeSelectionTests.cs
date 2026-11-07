using System;
using System.Collections;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using BuckShot.Base.Core;
using BuckShot.Base.Interfaces;
using BuckShot.Editor.Drawing;
using BuckShot.Editor.Drawing.Fields;
using Xunit;

namespace BuckShot.Tests.Editor
{
    [Collection(TestCollections.InputState)]
    public class MarqueeSelectionTests
    {
        public MarqueeSelectionTests() => TestEnvironment.EnsureInitialized();
        private static T Field<T>(object owner, string name) => (T)owner.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(owner);
        private static int Undos(Surface s) => Field<ICollection>(s, "_undoStack").Count;
        private static RectangleContainer Box(Surface s, int x, int y = 30, int size = 20)
        {
            var box = new RectangleContainer(s) { Left = x, Top = y, Width = size, Height = size };
            box.SetFieldValue(FieldType.SHADOW, false); box.SetFieldValue(FieldType.FILL_COLOR, Color.Blue);
            s.AddElement(box, false); return box;
        }
        private static void Select(Surface s, params DrawableContainer[] objects)
        { s.DeselectAllElements(); s.SelectElements(new DrawableContainerList(objects)); }

        [Fact]
        public void ReverseZoomedBoxSelection_ReplacesSelectionAndNeverExportsOrAddsUndo()
        {
            Sta(() =>
            {
                using var s = new Surface(new Bitmap(300, 300)); var a = Box(s, 30); var b = Box(s, 100); Select(s, b); s.ZoomFactor = new Fraction(2, 1);
                int history = Undos(s); s.Modified = false;
                Mouse(s, "SurfaceMouseDown", 140, 140); Mouse(s, "SurfaceMouseMove", 20, 20);
                Assert.Same(a, Assert.Single(s.SelectedElements));
                using (var export = (Bitmap)s.GetImageForExport()) Assert.Equal(0, export.GetPixel(10, 10).A);
                Mouse(s, "SurfaceMouseUp", 20, 20);
                Assert.False(s.Capture); Assert.Equal(history, Undos(s)); Assert.False(s.Modified);
            });
        }

        [Fact]
        public void ShiftBoxSelection_TogglesEntireEnclosedGroupsAgainstInitialSelection()
        {
            Sta(() =>
            {
                using var s = new Surface(new Bitmap(300, 300)); var a = Box(s, 30); var b = Box(s, 100); var c = Box(s, 180);
                Select(s, a, b); s.GroupSelectedElements(); s.DeselectAllElements();
                s.BeginSelectionBox(Point.Empty, Keys.None); Mouse(s, "SurfaceMouseMove", 80, 80); Mouse(s, "SurfaceMouseUp", 80, 80); Assert.Empty(s.SelectedElements);
                s.BeginSelectionBox(Point.Empty, Keys.None); Mouse(s, "SurfaceMouseMove", 140, 80); Mouse(s, "SurfaceMouseUp", 140, 80); Assert.Equal(2, s.SelectedElements.Count);
                s.SelectElement(c); s.BeginSelectionBox(Point.Empty, Keys.Shift);
                Mouse(s, "SurfaceMouseMove", 140, 80); Mouse(s, "SurfaceMouseMove", 145, 85); Assert.Same(c, Assert.Single(s.SelectedElements));
                Mouse(s, "SurfaceMouseUp", 145, 85); Assert.Same(c, Assert.Single(s.SelectedElements));
            });
        }

        [Theory]
        [InlineData("Escape")]
        [InlineData("Tool")]
        [InlineData("Document")]
        [InlineData("Capture")]
        public void BoxCancellation_ReleasesCaptureAndRestoresSelectionWhereAppropriate(string method)
        {
            Sta(() =>
            {
                using var s = new Surface(new Bitmap(300, 300)); var a = Box(s, 30); var b = Box(s, 100); Select(s, b); int history = Undos(s);
                s.BeginSelectionBox(Point.Empty, Keys.None); Mouse(s, "SurfaceMouseMove", 80, 80); Assert.Same(a, Assert.Single(s.SelectedElements));
                if (method == "Escape") Assert.True(s.ProcessCanvasKey(Keys.Escape));
                if (method == "Document") Assert.True(s.CancelSelectionBox());
                if (method == "Capture") s.Capture = false;
                if (method == "Tool") s.DrawingMode = DrawingModes.Line;
                if (method != "Tool") Assert.Same(b, Assert.Single(s.SelectedElements));
                Assert.False(s.Capture); Assert.Null(Field<Point?>(s, "_selectionBoxStart")); Assert.Equal(history, Undos(s));
            });
        }

        [Fact]
        public void SelectionThresholdAndObjectDrag_PreserveExistingPriorities()
        {
            Sta(() =>
            {
                using var s = new Surface(new Bitmap(300, 300)); var box = Box(s, 30); Select(s, box); s.ZoomFactor = new Fraction(2, 1);
                s.BeginSelectionBox(Point.Empty, Keys.Shift); Mouse(s, "SurfaceMouseMove", 2, 2); Assert.False(Field<bool>(s, "_selectionBoxActive"));
                Mouse(s, "SurfaceMouseUp", 2, 2); Assert.Same(box, Assert.Single(s.SelectedElements));
                Mouse(s, "SurfaceMouseDown", 80, 80); Mouse(s, "SurfaceMouseMove", 100, 100); Mouse(s, "SurfaceMouseUp", 100, 100);
                Assert.Equal(40, box.Left); Assert.Equal(40, box.Top); Assert.Null(Field<Point?>(s, "_selectionBoxStart"));
            });
        }

        [Fact]
        public void MembershipChangesNotifyOnceAndRetainUnchangedBindings()
        {
            Sta(() =>
            {
                using var surface = new Surface(new Bitmap(500, 400));
                var first = Box(surface, 30);
                var second = Box(surface, 100);
                Select(surface, first);
                int retainedChanges = 0;
                first.PropertyChanged += (sender, args) => { if (args.PropertyName == "Selected") retainedChanges++; };
                var notifications = new System.Collections.Generic.List<object[]>();
                surface.MovingElementChanged += (sender, args) => notifications.Add(args.Elements.Cast<object>().ToArray());
                surface.BeginSelectionBox(Point.Empty, Keys.None);
                Mouse(surface, "SurfaceMouseMove", 80, 80);
                Assert.Empty(notifications);
                Mouse(surface, "SurfaceMouseMove", 160, 80);
                Assert.Equal(new object[] { first, second }, Assert.Single(notifications));
                Assert.Equal(0, retainedChanges);
                var bound = Field<BuckShot.Base.Interfaces.Drawing.IDrawableContainerList>(surface.FieldAggregator, "_boundContainers");
                Assert.Equal(new object[] { first, second }, bound.Cast<object>().ToArray());
                Mouse(surface, "SurfaceMouseMove", 165, 85);
                Assert.Single(notifications);
                Mouse(surface, "SurfaceMouseMove", 80, 80);
                Assert.Equal(2, notifications.Count);
                Assert.Same(first, Assert.Single(notifications[1]));
                Assert.Same(first, Assert.Single(bound));
                Assert.Equal(0, retainedChanges);
                Mouse(surface, "SurfaceMouseUp", 80, 80);
                Assert.Equal(2, notifications.Count);
            });
        }

        [Fact]
        public void MarqueeRepaintsOnlyOldAndNewZoomedDirtyRegions()
        {
            Sta(() =>
            {
                using var surface = new Surface(new Bitmap(1000, 800));
                using var form = new Form();
                form.Controls.Add(surface); form.Show(); Application.DoEvents();
                surface.ZoomFactor = new Fraction(2, 1);
                surface.BeginSelectionBox(new Point(10, 10), Keys.None);
                var dirty = new System.Collections.Generic.List<Rectangle>();
                surface.Invalidated += (sender, args) => dirty.Add(args.InvalidRect);
                Mouse(surface, "SurfaceMouseMove", 100, 100);
                Mouse(surface, "SurfaceMouseMove", 140, 120);
                Assert.NotEmpty(dirty);
                Assert.All(dirty, bounds => Assert.True(bounds.Width < 200 && bounds.Height < 200, bounds.ToString()));
                Assert.Contains(dirty, bounds => bounds.Contains(20, 20) && bounds.Contains(100, 100));
                dirty.Clear();
                surface.CancelSelectionBox();
                Assert.NotEmpty(dirty);
                Assert.All(dirty, bounds => Assert.True(bounds.Width < 200 && bounds.Height < 200, bounds.ToString()));
            });
        }

        [Fact]
        public void CaptureChangeWithoutOwnedGesturePreservesOrdinaryObjectDrag()
        {
            Sta(() =>
            {
                using var surface = new Surface(new Bitmap(300, 300));
                var box = Box(surface, 30, 30, 60);
                Mouse(surface, "SurfaceMouseDown", 60, 60);
                Assert.True(Field<bool>(surface, "_mouseDown"));
                typeof(Surface).GetMethod("OnMouseCaptureChanged", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(surface, new object[] { EventArgs.Empty });
                Assert.True(Field<bool>(surface, "_mouseDown"));
                Assert.Same(box, Field<object>(surface, "_mouseDownElement"));
                Assert.Equal(BuckShot.Base.Interfaces.Drawing.EditStatus.MOVING, box.Status);
                Mouse(surface, "SurfaceMouseUp", 60, 60);
            });
        }

        [Fact]
        public void BlankCanvasMarqueeRemovesExistingCropAfterHandleRouting()
        {
            Sta(() =>
            {
                using var surface = new Surface(new Bitmap(400, 300));
                var crop = new CropContainer(surface) { Left = 50, Top = 50, Width = 80, Height = 80 };
                surface.AddElement(crop, false);
                typeof(Surface).GetField("_cropContainer", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(surface, crop);
                Mouse(surface, "SurfaceMouseDown", 250, 220);
                Assert.Null(Field<object>(surface, "_cropContainer"));
                Assert.DoesNotContain(crop, surface.Elements);
                Assert.NotNull(Field<Point?>(surface, "_selectionBoxStart"));
                Mouse(surface, "SurfaceMouseUp", 250, 220);
            });
        }

        private static void Mouse(Surface s, string method, int x, int y) => typeof(Surface).GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(s, new object[] { s, new MouseEventArgs(MouseButtons.Left, 1, x, y, 0) });
        private static void Sta(Action action)
        {
            Exception error = null; var thread = new Thread(() => { try { action(); } catch (Exception e) { error = e; } });
            thread.SetApartmentState(ApartmentState.STA); thread.Start(); Assert.True(thread.Join(TimeSpan.FromSeconds(30)), "UI test timed out");
            if (error != null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error).Throw();
        }
    }
}
