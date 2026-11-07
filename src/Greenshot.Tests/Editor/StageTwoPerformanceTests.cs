using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Greenshot.Base.Core;
using Greenshot.Editor.Drawing;
using Greenshot.Editor.Drawing.Fields;
using Xunit;

namespace Greenshot.Tests.Editor
{
    [Collection(TestCollections.InputState)]
    public class StageTwoPerformanceTests
    {
        public StageTwoPerformanceTests() => TestEnvironment.EnsureInitialized();
        private static LineContainer Target(Surface surface, int x, int y)
        {
            var target = new LineContainer(surface) { Left = x, Top = y, Width = 0, Height = 0 };
            surface.AddElement(target, false); return target;
        }
        private static T Field<T>(Surface surface, string name) => (T)typeof(Surface).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(surface);

        [Fact]
        public void TargetsBuildOncePerGestureAndRebuildOnlyForStationarySceneChanges()
        {
            DimensionFormatTests.Sta(() =>
            {
                using var surface = new Surface(new Bitmap(600, 400));
                var stationary = Target(surface, 100, 100);
                var moving = Target(surface, 20, 20);
                surface.SetObjectSnapping(true, false);
                Assert.Equal(new Point(100, 100), surface.SnapGesture(new Point(103, 100), null, moving, false));
                int builds = surface.SnapTargetBuildCount;
                for (int i = 0; i < 10; i++)
                {
                    moving.Left++;
                    Assert.Equal(new Point(100, 100), surface.SnapGesture(new Point(104, 100), null, moving, false));
                }
                Assert.Equal(builds, surface.SnapTargetBuildCount);
                surface.SnapGesture(new Point(moving.Left, moving.Top), null, moving, false);
                Assert.Null(Field<PointF?>(surface, "_snapTarget"));
                stationary.Left += 20;
                Assert.Equal(new Point(120, 100), surface.SnapGesture(new Point(123, 100), null, moving, false));
                Assert.Equal(++builds, surface.SnapTargetBuildCount);
                var added = Target(surface, 200, 100);
                Assert.Equal(new Point(200, 100), surface.SnapGesture(new Point(203, 100), null, moving, false));
                Assert.Equal(++builds, surface.SnapTargetBuildCount);
                surface.RemoveElement(added);
                Assert.Equal(new Point(203, 100), surface.SnapGesture(new Point(203, 100), null, moving, false));
                Assert.Equal(++builds, surface.SnapTargetBuildCount);
                surface.Undo();
                Assert.Equal(new Point(200, 100), surface.SnapGesture(new Point(203, 100), null, moving, false));
                Assert.Equal(++builds, surface.SnapTargetBuildCount);
                surface.ClearSnapGuide();
                Assert.Empty(Field<List<DrawableContainer>>(surface, "_snapStationary"));
                surface.SnapGesture(new Point(203, 100), null, moving, false);
                Assert.Equal(++builds, surface.SnapTargetBuildCount);
            });
        }

        [Fact]
        public void SnapLockRetainsFeatureUntilReleaseRadiusAndRespectsModifiersAndPointPriority()
        {
            DimensionFormatTests.Sta(() =>
            {
                var previous = InputState.ModifierKeysSource;
                Keys modifiers = Keys.None;
                InputState.ModifierKeysSource = () => modifiers;
                try
                {
                    using var surface = new Surface(new Bitmap(500, 400));
                    Target(surface, 100, 100); Target(surface, 114, 100);
                    surface.SetObjectSnapping(true, false);
                    Assert.Equal(new Point(100, 100), surface.SnapGesture(new Point(101, 100), null, null, false));
                    Assert.Equal(new Point(100, 100), surface.SnapGesture(new Point(108, 100), null, null, false));
                    Assert.Equal(new Point(100, 100), surface.SnapGesture(new Point(110, 100), null, null, false));
                    Assert.Equal(new Point(114, 100), surface.SnapGesture(new Point(111, 100), null, null, false));
                    surface.ClearSnapGuide();
                    surface.SnapGesture(new Point(101, 100), null, null, false);
                    modifiers = Keys.Shift;
                    Assert.Equal(new Point(114, 100), surface.SnapGesture(new Point(108, 100), null, null, false));
                    Assert.Equal(new Point(108, 100), surface.SnapGesture(new Point(108, 100), null, null, false, p => false));
                    using var rays = new Surface(new Bitmap(500, 400));
                    Target(rays, 100, 100); Target(rays, 160, 152);
                    modifiers = Keys.None; rays.SetObjectSnapping(true, true);
                    rays.SnapGesture(new Point(150, 152), Point.Empty, null, true);
                    Assert.NotNull(Field<Point?>(rays, "_snapAnchor"));
                    Assert.Equal(new Point(160, 152), rays.SnapGesture(new Point(154, 152), Point.Empty, null, true));
                    Assert.Null(Field<Point?>(rays, "_snapAnchor"));
                    modifiers = Keys.Control;
                    Assert.Equal(new Point(220, 222), rays.SnapGesture(new Point(220, 222), Point.Empty, null, true));
                }
                finally { InputState.ModifierKeysSource = previous; }
            });
        }

        [Fact]
        public void GuideInvalidatesOnlyChangedCompleteDeviceBounds()
        {
            DimensionFormatTests.Sta(() =>
            {
                using var form = new Form(); using var surface = new Surface(new Bitmap(1000, 800));
                form.Controls.Add(surface); form.Show(); Application.DoEvents();
                Target(surface, 100, 100); surface.SetObjectSnapping(true, true);
                surface.SnapGesture(new Point(103, 100), null, null, false);
                var dirty = new List<Rectangle>();
                surface.Invalidated += (s, e) => dirty.Add(e.InvalidRect);
                surface.SnapGesture(new Point(104, 100), null, null, false);
                Assert.Empty(dirty);
                surface.ClearSnapGuide(); dirty.Clear();
                surface.SnapGesture(new Point(150, 152), Point.Empty, null, true);
                Assert.Contains(dirty, bounds => bounds.Contains(1, 1) && bounds.Contains(140, 140));
                Assert.All(dirty, bounds => Assert.True(bounds.Width < 250 && bounds.Height < 250));
                dirty.Clear(); surface.SnapGesture(new Point(150, 152), Point.Empty, null, true);
                Assert.Empty(dirty);
                surface.ClearSnapGuide(); Assert.NotEmpty(dirty);
                Assert.All(dirty, bounds => Assert.True(bounds.Width < 250 && bounds.Height < 250));
            });
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(3, 2)]
        [InlineData(2, 3)]
        public void RetainedVisualDirtyRegionsMatchFullRenderAcrossRotationResizeUndoAndGroupMove(int numerator, int denominator)
            => ExerciseRetainedVisuals(numerator, denominator, false);

        [Theory]
        [InlineData(1, 1)]
        [InlineData(3, 2)]
        [InlineData(2, 3)]
        [InlineData(4, 1)]
        public void SquareQuarterTurnUndoRepaintsIdentityRotationHandleAndStem(int numerator, int denominator)
            => ExerciseRetainedVisuals(numerator, denominator, true);

        private static void ExerciseRetainedVisuals(int numerator, int denominator, bool square)
        {
            DimensionFormatTests.Sta(() =>
            {
                using var background = new Bitmap(650, 450);
                using (var graphics = Graphics.FromImage(background)) graphics.Clear(Color.White);
                using var surface = new Surface((Bitmap)background.Clone());
                using var form = new Form { Size = new Size(1100, 800) };
                form.Controls.Add(surface); form.Show(); Application.DoEvents();
                surface.ZoomFactor = new Fraction((uint)numerator, (uint)denominator);
                var box = new RectangleContainer(surface) { Left = 160, Top = 170, Width = 90, Height = square ? 90 : 55 };
                box.SetFieldValue(FieldType.FILL_COLOR, Color.Red); box.SetFieldValue(FieldType.SHADOW, true);
                surface.AddElement(box, false); surface.SelectElement(box); if (!square) box.SetDimensionVisible(0, true);
                using var content = new Bitmap(45, 30);
                using (var graphics = Graphics.FromImage(content)) graphics.Clear(Color.Green);
                var image = new ImageContainer(surface) { Image = content, Left = 330, Top = 200, Width = 45, Height = 30 };
                surface.AddElement(image, false);
                var ownedImage = image.Image;
                using var incremental = new Bitmap(surface.Width, surface.Height);
                surface.DrawToBitmap(incremental, new Rectangle(Point.Empty, incremental.Size));
                var dirty = new List<Rectangle>();
                surface.Invalidated += (s, e) => dirty.Add(e.InvalidRect);
                string folder = System.IO.Path.Combine(Environment.GetEnvironmentVariable("GITHUB_WORKSPACE") ?? System.IO.Path.GetTempPath(), "TestResults", "stage2-render");
                System.IO.Directory.CreateDirectory(folder);
                string scenario = $"{(square ? "square" : "scene")}-{numerator}-{denominator}";
                void Compare(string phase, Action mutation, bool bounded = false)
                {
                    using var before = new Bitmap(incremental);
                    dirty.Clear(); mutation();
                    Assert.NotEmpty(dirty);
                    if (bounded) Assert.DoesNotContain(dirty, rectangle => rectangle.Contains(surface.ClientRectangle));
                    foreach (var rectangle in dirty.ToArray())
                    {
                        // A WinForms paint has an immutable device clip; emulate it by copying back only the dirty patch.
                        using var patch = new Bitmap(incremental);
                        using (var graphics = Graphics.FromImage(patch))
                        {
                            graphics.SetClip(rectangle);
                            typeof(Surface).GetMethod("SurfacePaint", BindingFlags.Instance | BindingFlags.NonPublic)
                                .Invoke(surface, new object[] { surface, new PaintEventArgs(graphics, rectangle) });
                        }
                        using var target = Graphics.FromImage(incremental);
                        target.SetClip(rectangle);
                        target.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                        target.DrawImageUnscaled(patch, Point.Empty);
                    }
                    using var full = new Bitmap(surface.Width, surface.Height);
                    surface.DrawToBitmap(full, new Rectangle(Point.Empty, full.Size));
                    var area = new Rectangle(Point.Empty, full.Size);
                    var expected = full.LockBits(area, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    var actual = incremental.LockBits(area, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    int mismatch = -1;
                    int stride = expected.Stride;
                    try
                    {
                        var expectedBytes = new byte[Math.Abs(expected.Stride) * full.Height];
                        var actualBytes = new byte[Math.Abs(actual.Stride) * full.Height];
                        System.Runtime.InteropServices.Marshal.Copy(expected.Scan0, expectedBytes, 0, expectedBytes.Length);
                        System.Runtime.InteropServices.Marshal.Copy(actual.Scan0, actualBytes, 0, actualBytes.Length);
                        for (int i = 0; i < expectedBytes.Length; i++)
                            if (expectedBytes[i] != actualBytes[i]) { mismatch = i; break; }
                    }
                    finally { full.UnlockBits(expected); incremental.UnlockBits(actual); }
                    if (mismatch >= 0)
                    {
                        string prefix = System.IO.Path.Combine(folder, scenario + "-" + phase);
                        int y = mismatch / Math.Abs(stride), x = (mismatch % Math.Abs(stride)) / 4;
                        before.Save(prefix + "-before.png");
                        incremental.Save(prefix + "-patched.png");
                        full.Save(prefix + "-full.png");
                        string evidence = $"Phase: {phase}; first mismatch ({x}, {y}); expected {full.GetPixel(x, y).ToArgb():X8}; actual {incremental.GetPixel(x, y).ToArgb():X8}; dirty: {string.Join("; ", dirty)}";
                        System.IO.File.WriteAllText(prefix + "-diagnostic.txt", evidence);
                        Assert.Fail(evidence);
                    }
                }
                Compare("rotate", () => surface.RotateSelectedObjects(square ? 90 : 17), bounded: true);
                if (!square) Compare("resize", () => { box.Invalidate(); box.Width += 13; box.Invalidate(); }, bounded: true);
                Compare("undo", () => surface.Undo());
                Compare("redo", () => surface.Redo());
                if (square) { incremental.Save(System.IO.Path.Combine(folder, scenario + ".png")); return; }
                Compare("group", () => { surface.SelectElement(image); surface.GroupSelectedElements(); });
                Compare("group-rotate", () => surface.RotateSelectedObjects(12));
                Compare("group-move", () => surface.SelectedElements.MoveBy(13, -9));
                Compare("group-undo", () => surface.Undo());
                Assert.Same(ownedImage, image.Image);
                incremental.Save(System.IO.Path.Combine(folder, scenario + ".png"));
            });
        }
    }
}
