using System;
using BuckShot.Base.Interfaces.Drawing.Adorners;
using BuckShot.Base.Core;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Dapplo.Windows.Common.Structs;
using BuckShot.Base.Interfaces.Drawing;
using BuckShot.Base.Interfaces;
using BuckShot.Editor.Helpers;

namespace BuckShot.Editor.Drawing
{
    public sealed partial class Surface
    {
        [NonSerialized] private bool _snapPoints, _snapThrough;
        [NonSerialized] private int _snappingRadius = 8;
        [NonSerialized] private PointF? _snapTarget;
        [NonSerialized] private Point? _snapAnchor, _snapEnd;
        internal void SetObjectSnapping(bool points, bool through, int radius = 8)
        {
            _snappingRadius = Math.Max(1, Math.Min(100, radius));
            _snapPoints = points; _snapThrough = through; ClearSnapGuide();
        }
        internal void ClearSnapGuide()
        {
            SetSnapGuide(null, null, null);
            ReleaseSnapTargets();
        }
        internal Point SnapGesture(Point pointer, Point? anchor, IDrawableContainer owner, bool through, Func<Point, bool> compatible = null)
        {
            if (!_snapPoints && !_snapThrough) { SetSnapGuide(null, null, null); return pointer; }
            var targets = SnapTargets(new[] { owner });
            PrepareSnapLock("endpoint");
            bool useThrough = through && _snapThrough && (InputState.ModifierKeys & Keys.Control) == 0;
            if (!TryRetainedSnap(pointer, anchor, owner, 0, _snapPoints, useThrough, compatible, out var end, out var target, out bool ray))
                end = ObjectSnapHelper.Snap(pointer, anchor, targets, _snapPoints, useThrough, _zoomFactor, compatible,
                    out target, out ray, _snappingRadius);
            LockSnap(target, owner, 0, ray);
            SetSnapGuide(target, ray ? anchor : null, ray ? end : (Point?)null);
            return end;
        }
        internal Point SnapDrawingStart(Point pointer, IDrawableContainer owner)
            => ObjectSnapHelper.Supported(owner) ? SnapGesture(pointer, null, owner, false) : pointer;

        internal NativeRectFloat SnapDrawingBounds(IDrawableContainer owner, NativeRect original, Point pointer,
            NativeRectFloat constrained, IDoubleProcessor angle, ScaleOptions? options)
        {
            if (!ObjectSnapHelper.Supported(owner) || (!_snapPoints && !_snapThrough)) return constrained;
            if (!ObjectSnapHelper.IsLine(owner) && _snapPoints)
                return SnapShapeDrawing(owner, original, pointer, constrained, angle, options);
            var anchor = new Point(original.X, original.Y);
            var actualOptions = options ?? ScaleHelper.GetScaleOptions();
            bool Compatible(Point p)
            {
                var scaled = ScaleHelper.Scale(new NativeRect(original.X, original.Y, p.X - original.X, p.Y - original.Y), p.X, p.Y, angle, actualOptions);
                return Math.Abs(scaled.X + scaled.Width - p.X) <= 1 && Math.Abs(scaled.Y + scaled.Height - p.Y) <= 1;
            }
            var end = SnapGesture(pointer, anchor, owner, ObjectSnapHelper.IsLine(owner), Compatible);
            return end == pointer ? constrained : ScaleHelper.Scale(new NativeRect(original.X, original.Y, end.X - original.X, end.Y - original.Y), end.X, end.Y, angle, actualOptions);
        }

        // Resize the provisional bounds about the original drawing anchor. Each feature
        // supplies a possible endpoint correction; apply normal modifiers before accepting it.
        private NativeRectFloat SnapShapeDrawing(IDrawableContainer owner, NativeRect original, Point pointer,
            NativeRectFloat baseline, IDoubleProcessor angle, ScaleOptions? options)
        {
            var targets = SnapTargets(new[] { owner });
            PrepareSnapLock("shape");
            PointF? nextTarget = null;
            int nextSource = -1;
            var opts = options ?? ScaleHelper.GetScaleOptions();
            NativeRectFloat Scale(Point p, ScaleOptions o) => ScaleHelper.Scale(
                new NativeRect(original.X, original.Y, p.X-original.X, p.Y-original.Y), p.X, p.Y, angle, o);
            PointF Feature(NativeRectFloat b, float x, float y) => new PointF(b.X+b.Width*x, b.Y+b.Height*y);
            var raw = Scale(pointer, opts & ~ScaleOptions.Rational);
            var rawX = Scale(new Point(pointer.X+2,pointer.Y), opts & ~ScaleOptions.Rational);
            var rawY = Scale(new Point(pointer.X,pointer.Y+2), opts & ~ScaleOptions.Rational);
            double best = Math.Pow(_snappingRadius / Math.Max(.001, (double)_zoomFactor), 2);
            var result = baseline;
            foreach (float x in new[] { 0f, .5f, 1f })
            foreach (float y in new[] { 0f, .5f, 1f })
            {
                var source = Feature(baseline,x,y);
                var origin = Feature(raw,x,y);
                double ax = (Feature(rawX,x,y).X-origin.X)/2;
                double ay = (Feature(rawY,x,y).Y-origin.Y)/2;
                if (ax == 0 && ay == 0) continue;
                foreach (var target in targets)
                {
                    double distance = ObjectSnapHelper.DistanceSquared(source,target);
                    bool retained = _snapLockedTarget >= 0 && ReferenceEquals(_snapLockedOwner, owner) &&
                        _snapLockedSource == (int)(x * 2) * 3 + (int)(y * 2) && targets[_snapLockedTarget] == target;
                    double limit = retained ? Math.Pow(_snappingRadius * 1.25 / Math.Max(.001, (double)_zoomFactor), 2) : best;
                    if (distance > limit || (!retained && nextTarget != null && distance >= best)) continue;
                    if ((ax == 0 && Math.Abs(target.X-source.X) > .5) || (ay == 0 && Math.Abs(target.Y-source.Y) > .5)) continue;
                    var end = new Point(pointer.X+(ax == 0 ? 0 : (int)Math.Round((target.X-source.X)/ax)),
                        pointer.Y+(ay == 0 ? 0 : (int)Math.Round((target.Y-source.Y)/ay)));
                    var candidate = Scale(end,opts);
                    if (ObjectSnapHelper.DistanceSquared(Feature(candidate,x,y),target) > 2) continue;
                    best=distance; result=candidate; nextTarget=target; nextSource=(int)(x * 2) * 3 + (int)(y * 2);
                    if (retained)
                    {
                        LockSnap(nextTarget, owner, nextSource, false);
                        SetSnapGuide(nextTarget, null, null);
                        return result;
                    }
                }
            }
            LockSnap(nextTarget, owner, nextSource, false);
            SetSnapGuide(nextTarget, null, null);
            return result;
        }

        internal Point PreviewDrawingStart(Point pointer)
        {
            if (!SupportsDrawingStartSnapping(DrawingMode))
            { ClearSnapGuide(); return pointer; }
            return SnapGesture(pointer, null, null, false);
        }

        internal static bool SupportsDrawingStartSnapping(DrawingModes mode) => mode == DrawingModes.Line || mode == DrawingModes.Arrow
            || mode == DrawingModes.Rect || mode == DrawingModes.Ellipse || mode == DrawingModes.Text || mode == DrawingModes.Scale || mode == DrawingModes.Polygon;

        [NonSerialized] private IAdorner _existingGestureAdorner;
        internal bool CanAttractDrawingPointer => SupportsDrawingStartSnapping(DrawingMode) && _existingGestureAdorner == null &&
            _vertexOwner == null && !_selectionBoxStart.HasValue && _mouseDownElement == null && !_mouseDown &&
            !selectedElements.SelectMany(element => element.Adorners).Any(adorner => adorner.IsActive);
        private void EndExistingHandleGesture()
        {
            var adorner = _existingGestureAdorner; _existingGestureAdorner = null;
            adorner?.MouseUp(this, new MouseEventArgs(MouseButtons.Left, 0, 0, 0, 0));
            if (adorner != null) Capture = false;
            ClearSnapGuide();
        }
        private void ApplySnappedPointer(Point pointer, Point snapped)
        {
            // Only a visible editor may move the real pointer. Headless geometry/event
            // tests and inactive documents still calculate their previews normally.
            if (!CanAttractDrawingPointer || _snapTarget == null || pointer == snapped || !IsHandleCreated || FindForm()?.Visible != true || !ClientRectangle.Contains(PointToClient(MousePosition))) return;
            System.Windows.Forms.Cursor.Position = PointToScreen(new Point(
                (int)Math.Round(snapped.X * (double)_zoomFactor), (int)Math.Round(snapped.Y * (double)_zoomFactor)));
        }

        internal NativeRectFloat SnapHandleBounds(IDrawableContainer owner, NativeRect original, Positions position, Point pointer,
            NativeRectFloat constrained, ScaleOptions? options)
        {
            if (owner is DrawableContainer transformed && transformed.HasObjectTransform)
                return SnapRotatedHandle(transformed, original, position, pointer, options);
            if (!ObjectSnapHelper.Supported(owner) || (!_snapPoints && !_snapThrough)) return constrained;
            bool line = ObjectSnapHelper.IsLine(owner);
            if (line) options = (options ?? ScaleHelper.GetScaleOptions()) & ~ScaleOptions.Rational;
            var anchor = position == Positions.TopLeft ? new Point(original.X + original.Width, original.Y + original.Height) : new Point(original.X, original.Y);
            // Line endpoint Shift uses the same 15-degree constraint as line drawing.
            Point Constrain(Point p) => line && (InputState.ModifierKeys & Keys.Shift) != 0 ? GetScaleEndpoint(anchor, p, InputState.ModifierKeys) : p;
            NativeRectFloat Scale(Point p) => ScaleHelper.Scale(original, position, new NativePointFloat(p.X, p.Y), options);
            Point Handle(NativeRectFloat b) => new Point(
                (int)(position == Positions.TopLeft || position == Positions.MiddleLeft || position == Positions.BottomLeft ? b.X : position == Positions.TopCenter || position == Positions.BottomCenter ? b.X + b.Width / 2 : b.X + b.Width),
                (int)(position == Positions.TopLeft || position == Positions.TopCenter || position == Positions.TopRight ? b.Y : position == Positions.MiddleLeft || position == Positions.MiddleRight ? b.Y + b.Height / 2 : b.Y + b.Height));
            var baseline = Constrain(pointer);
            bool Compatible(Point p) => ObjectSnapHelper.DistanceSquared(Constrain(p), p) <= 2 && ObjectSnapHelper.DistanceSquared(Handle(Scale(p)), p) <= 2;
            var end = SnapGesture(baseline, line ? anchor : (Point?)null, owner, line, Compatible);
            return line ? Scale(Constrain(end)) : end == baseline && baseline == pointer ? constrained : Scale(end);
        }

        private Point SnapTranslation(Point pointer)
        {
            if (!_snapPoints) { SetSnapGuide(null, null, null); return pointer; }
            var moving = _mouseDownElement.Selected ? selectedElements.ToArray() : new[] { _mouseDownElement };
            var targets = SnapTargets(moving);
            PrepareSnapLock("translation");
            PointF? nextTarget = null;
            IDrawableContainer nextOwner = null;
            int nextSource = -1;
            int dx = pointer.X - _mouseStart.X, dy = pointer.Y - _mouseStart.Y;
            double best = double.MaxValue;
            Point correction = Point.Empty;
            foreach (var member in moving)
            {
                int sourceIndex = -1;
                foreach (var source in ObjectSnapHelper.Targets(member))
                {
                    sourceIndex++;
                    var translated = Point.Round(new PointF(source.X + dx, source.Y + dy));
                    if (TryRetainedSnap(translated, null, member, sourceIndex, true, false, null, out var retained, out var retainedTarget, out _))
                    {
                        LockSnap(retainedTarget, member, sourceIndex, false);
                        SetSnapGuide(retainedTarget, null, null);
                        return new Point(pointer.X + retained.X - translated.X, pointer.Y + retained.Y - translated.Y);
                    }
                    var end = ObjectSnapHelper.Snap(translated, null, targets, true, false, _zoomFactor, null, out var target, out _, _snappingRadius);
                    double distance = ObjectSnapHelper.DistanceSquared(translated, end);
                    if (target != null && distance < best)
                    { best = distance; correction = new Point(end.X - translated.X, end.Y - translated.Y); nextTarget = target; nextOwner = member; nextSource = sourceIndex; }
                }
            }
            LockSnap(nextTarget, nextOwner, nextSource, false);
            SetSnapGuide(nextTarget, null, null);
            return new Point(pointer.X + correction.X, pointer.Y + correction.Y);
        }

        private void PaintSnapGuide(Graphics graphics)
        {
            if (_snapTarget == null) return;
            float zoom = (float)(double)_zoomFactor;
            var target = new PointF(_snapTarget.Value.X * zoom, _snapTarget.Value.Y * zoom);
            using var pen = new Pen(Color.DeepSkyBlue, 1) { DashStyle = DashStyle.Dot };
            if (_snapAnchor != null && _snapEnd != null)
                graphics.DrawLine(pen, _snapAnchor.Value.X * zoom, _snapAnchor.Value.Y * zoom, _snapEnd.Value.X * zoom, _snapEnd.Value.Y * zoom);
            if (_snapGuideEditing)
            {
                // Existing geometry moves to this target; a small cross cannot be mistaken for the pointer.
                using var editingPen = new Pen(Color.SlateGray, 1);
                graphics.DrawLine(editingPen, target.X - 2, target.Y, target.X + 2, target.Y);
                graphics.DrawLine(editingPen, target.X, target.Y - 2, target.X, target.Y + 2);
                return;
            }
            graphics.FillEllipse(Brushes.White, target.X - 3, target.Y - 3, 6, 6);
            graphics.DrawEllipse(Pens.DeepSkyBlue, target.X - 3, target.Y - 3, 6, 6);
        }
    }
}
