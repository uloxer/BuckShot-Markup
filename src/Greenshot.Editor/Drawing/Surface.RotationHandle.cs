using System;
using System.Drawing;
using System.Windows.Forms;
using Greenshot.Base.Interfaces;
using Greenshot.Base.Interfaces.Drawing;
using Greenshot.Editor.Memento;
using Greenshot.Base.Core;

namespace Greenshot.Editor.Drawing
{
    public sealed partial class Surface
    {
        private DrawableContainer _rotationOwner;
        private double[] _rotationOriginal;
        private double _rotationStartPointer, _rotationDelta;
        private PointF _rotationPivot;
        private float RotationScreenScale => (float)(CurrentDpi / 96d / (double)_zoomFactor);
        private DrawableContainer RotationHandleOwner => selectedElements.Count == 1 &&
            selectedElements[0] is DrawableContainer shape && _elements.Contains(shape) && shape.GroupId == Guid.Empty && SupportsObjectRotation(shape) ? shape : null;

        internal bool TryGetRotationHandle(out PointF point)
        {
            var owner = RotationHandleOwner;
            if (owner == null) { point = PointF.Empty; return false; }
            var bounds = owner.ModelBounds;
            var top = owner.ModelToImage(new PointF(bounds.Left + bounds.Width / 2f, bounds.Top));
            var above = owner.ModelToImage(new PointF(bounds.Left + bounds.Width / 2f, bounds.Top - 1));
            double dx = above.X - top.X, dy = above.Y - top.Y, length = Math.Sqrt(dx * dx + dy * dy);
            point = new PointF(top.X + (float)(dx / length * 44 * RotationScreenScale), top.Y + (float)(dy / length * 44 * RotationScreenScale));
            if (Image != null)
            {
                // Keep the whole hit target reachable when the transformed upper edge faces outside the canvas.
                float marginX = Math.Min(12 * RotationScreenScale, Image.Width / 2f);
                float marginY = Math.Min(12 * RotationScreenScale, Image.Height / 2f);
                point = new PointF(Math.Max(marginX, Math.Min(Image.Width - marginX, point.X)),
                    Math.Max(marginY, Math.Min(Image.Height - marginY, point.Y)));
            }
            return true;
        }

        private bool RotationHandleHit(PointF point) => TryGetRotationHandle(out var handle) &&
            Math.Pow(point.X - handle.X, 2) + Math.Pow(point.Y - handle.Y, 2) <= Math.Pow(11 * RotationScreenScale, 2);

        private bool RotationMouseDown(MouseEventArgs e, PointF pointer)
        {
            if (e.Button != MouseButtons.Left || !RotationHandleHit(pointer)) return false;
            // A near-edge fallback must never steal an existing resize handle's hit area.
            if (FindActiveAdorner(e) is Adorners.ResizeAdorner resize)
            {
                // Resize bounds store a device-sized glyph around a model-space centre.
                // Its broad image-space hit test otherwise swallows the rotation handle at high zoom.
                var bounds = resize.Bounds;
                var center = RotationHandleOwner.ModelToImage(new PointF(bounds.Left + bounds.Width / 2f, bounds.Top + bounds.Height / 2f));
                double zoom = _zoomFactor;
                if (Math.Abs(pointer.X - center.X) * zoom <= bounds.Width / 2f + 3 &&
                    Math.Abs(pointer.Y - center.Y) * zoom <= bounds.Height / 2f + 3) return false;
            }
            var owner = RotationHandleOwner;
            if (owner is TextContainer text) text.EndTextEditingForRotation();
            _rotationOwner = owner;
            _rotationOriginal = owner.ObjectTransformState;
            var pivot = owner.ImageCentre; _rotationPivot = new PointF((float)pivot.X, (float)pivot.Y);
            _rotationStartPointer = Math.Atan2(pointer.Y - pivot.Y, pointer.X - pivot.X);
            _rotationDelta = 0; ClearSnapGuide(); Capture = true; Cursor = Cursors.Hand;
            return true;
        }

        private bool RotationMouseMove(MouseEventArgs e, PointF pointer)
        {
            if (_rotationOwner == null)
            {
                if (!RotationHandleHit(pointer)) return false;
                Cursor = Cursors.Hand; return true;
            }
            double dx = pointer.X - _rotationPivot.X, dy = pointer.Y - _rotationPivot.Y;
            if (dx * dx + dy * dy < 1) return true;
            _rotationDelta = (Math.Atan2(dy, dx) - _rotationStartPointer) * 180 / Math.PI;
            if ((InputState.ModifierKeys & Keys.Shift) != 0) _rotationDelta = Math.Round(_rotationDelta / 15) * 15;
            if (_rotationOwner.PreviewRotation(_rotationOriginal, _rotationDelta, _rotationPivot.X, _rotationPivot.Y)) Invalidate();
            return true;
        }

        private bool RotationMouseUp(MouseEventArgs e, PointF pointer)
        {
            if (_rotationOwner == null) return false;
            if (e.Button != MouseButtons.Left) { CancelRotationGesture(); return true; }
            RotationMouseMove(e, pointer);
            var owner = _rotationOwner; var delta = _rotationDelta;
            var original = _rotationOriginal;
            _rotationOwner = null; _rotationOriginal = null;
            if (Math.Abs(delta % 360) > .000001)
            {
                MakeUndoable(new DrawableContainerBoundsChangeMemento(owner, original), false);
                Modified = true;
            }
            Capture = false; Cursor = Cursors.Default; Invalidate();
            return true;
        }

        internal bool CancelRotationGesture()
        {
            if (_rotationOwner == null) return false;
            var owner = _rotationOwner; _rotationOwner = null;
            owner.RestoreObjectTransform(_rotationOriginal); _rotationOriginal = null;
            Capture = false; Cursor = Cursors.Default; Invalidate();
            return true;
        }

        private void PaintRotationHandle(Graphics graphics)
        {
            if (!TryGetRotationHandle(out var point)) return;
            var owner = RotationHandleOwner; var bounds = owner.ModelBounds;
            var top = owner.ModelToImage(new PointF(bounds.Left + bounds.Width / 2f, bounds.Top));
            var saved = graphics.Save();
            graphics.ScaleTransform(_zoomFactor, _zoomFactor);
            float scale = RotationScreenScale, radius = 7 * scale;
            using var pen = new Pen(SystemColors.Highlight, 2 * scale);
            using var fill = new SolidBrush(SystemColors.Window);
            graphics.DrawLine(pen, top, point);
            graphics.FillEllipse(fill, point.X - radius, point.Y - radius, radius * 2, radius * 2);
            graphics.DrawEllipse(pen, point.X - radius, point.Y - radius, radius * 2, radius * 2);
            graphics.DrawArc(pen, point.X - 3 * scale, point.Y - 3 * scale, 6 * scale, 6 * scale, 30, 270);
            graphics.DrawLine(pen, point.X + 3 * scale, point.Y, point.X + 3 * scale, point.Y - 3 * scale);
            graphics.Restore(saved);
        }
    }
}
