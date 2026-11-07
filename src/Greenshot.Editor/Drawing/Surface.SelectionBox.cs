using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Greenshot.Base.Interfaces.Drawing;
using Greenshot.Base.Interfaces;

namespace Greenshot.Editor.Drawing
{
    public sealed partial class Surface
    {
        private Point? _selectionBoxStart;
        private Point _selectionBoxEnd;
        private IDrawableContainer[] _selectionBoxInitial;
        private bool _selectionBoxActive, _selectionBoxToggle;
        private float SelectionBoxLineWidth => Math.Max(1f, CurrentDpi / 96f);

        internal void BeginSelectionBox(Point start, Keys modifiers)
        {
            _selectionBoxStart = start;
            _selectionBoxEnd = start;
            _selectionBoxInitial = selectedElements.ToArray();
            _selectionBoxToggle = (modifiers & Keys.Shift) != 0;
            _selectionBoxActive = false;
            ClearSnapGuide();
            HideFocusFrame();
            Focus();
            Capture = true;
        }

        private Rectangle SelectionBoxBounds => Rectangle.FromLTRB(Math.Min(_selectionBoxStart.Value.X, _selectionBoxEnd.X),
            Math.Min(_selectionBoxStart.Value.Y, _selectionBoxEnd.Y), Math.Max(_selectionBoxStart.Value.X, _selectionBoxEnd.X), Math.Max(_selectionBoxStart.Value.Y, _selectionBoxEnd.Y));

        private void SetBoxSelection(IEnumerable<IDrawableContainer> selection, bool groupsComplete = false)
        {
            var next = new HashSet<IDrawableContainer>(groupsComplete ? selection : CompleteGroups(selection.Where(_elements.Contains)));
            if (next.SetEquals(selectedElements)) return;
            var removed = selectedElements.Where(element => !next.Contains(element)).ToArray();
            var added = _elements.Where(element => next.Contains(element) && !selectedElements.Contains(element)).ToArray();
            InvalidateRotationSelectionHandle();
            InvalidateSelectionMembers(removed);
            foreach (var element in removed) DeselectSingleElement(element, false);
            foreach (var element in added) SelectSingleElement(element, false, false);

            // Keep document order without rebinding retained members or changing their Selected property.
            selectedElements.Clear();
            foreach (var element in _elements.Where(next.Contains)) selectedElements.Add(element);
            InvalidateSelectionMembers(added);
            InvalidateRotationSelectionHandle();
            _movingElementChanged?.Invoke(this, new SurfaceElementEventArgs { Elements = selectedElements });
        }

        private bool MoveSelectionBox(Point pointer)
        {
            if (!_selectionBoxStart.HasValue) return false;
            var previousBounds = _selectionBoxActive ? (Rectangle?)SelectionBoxBounds : null;
            _selectionBoxEnd = pointer;
            double dx = pointer.X - _selectionBoxStart.Value.X;
            double dy = pointer.Y - _selectionBoxStart.Value.Y;
            if (!_selectionBoxActive && (dx * dx + dy * dy) * Math.Pow((double)_zoomFactor, 2) < 16) return true;
            _selectionBoxActive = true;
            var bounds = SelectionBoxBounds;
            var next = _selectionBoxToggle ? new HashSet<IDrawableContainer>(_selectionBoxInitial) : new HashSet<IDrawableContainer>();
            var seen = new HashSet<Guid>();
            var groups = _elements.OfType<DrawableContainer>().Where(element => element.GroupId != Guid.Empty).ToLookup(element => element.GroupId);
            foreach (var member in _elements.OfType<DrawableContainer>().Where(e => e != _cropContainer && e.IsUndoable && !e.IsConfirmable))
            {
                if (member.GroupId != Guid.Empty && !seen.Add(member.GroupId)) continue;
                var unit = member.GroupId == Guid.Empty ? new[] { member } : groups[member.GroupId].ToArray();
                var visual = unit.Select(e => (Rectangle)e.Bounds).Aggregate(Rectangle.Union);
                if (visual.Left < bounds.Left || visual.Right > bounds.Right || visual.Top < bounds.Top || visual.Bottom > bounds.Bottom) continue;
                if (_selectionBoxToggle && _selectionBoxInitial.Contains(member))
                {
                    foreach (var element in unit) next.Remove(element);
                }
                else
                {
                    foreach (var element in unit) next.Add(element);
                }
            }
            SetBoxSelection(next, groupsComplete: true);
            if (previousBounds.HasValue) InvalidateSelectionBox(previousBounds.Value);
            InvalidateSelectionBox(bounds);
            return true;
        }

        private bool FinishSelectionBox(MouseEventArgs e)
        {
            if (!_selectionBoxStart.HasValue || e.Button != MouseButtons.Left) return false;
            MoveSelectionBox(e.Location);
            if (!_selectionBoxActive && !_selectionBoxToggle) SetBoxSelection(Array.Empty<IDrawableContainer>());
            if (_selectionBoxActive) InvalidateSelectionBox(SelectionBoxBounds);
            _selectionBoxStart = null;
            _selectionBoxInitial = null;
            _selectionBoxActive = false;
            Capture = false;
            return true;
        }

        internal bool CancelSelectionBox()
        {
            if (!_selectionBoxStart.HasValue) return false;
            var original = _selectionBoxInitial;
            if (_selectionBoxActive) InvalidateSelectionBox(SelectionBoxBounds);
            _selectionBoxStart = null;
            _selectionBoxInitial = null;
            _selectionBoxActive = false;
            SetBoxSelection(original);
            Capture = false;
            return true;
        }

        protected override void OnMouseCaptureChanged(EventArgs e)
        {
            base.OnMouseCaptureChanged(e);
            if (Capture) return;

            bool ownsCapture = _rotationOwner != null || _existingGestureAdorner != null ||
                _selectionBoxStart.HasValue || _vertexOwner != null;
            if (!ownsCapture) return;

            CancelRotationGesture();
            EndExistingHandleGesture();
            CancelSelectionBox();
            // Polygon creation spans several clicks; only an owned vertex drag is cancelled here.
            if (_vertexOwner != null) CancelPolygonInteraction();
            ClearSnapGuide();
            _mouseDown = false;
            if (_mouseDownElement != null) _mouseDownElement.Status = EditStatus.IDLE;
            _mouseDownElement = null;
        }

        private void InvalidateSelectionBox(Rectangle bounds)
        {
            // Cover the DPI-scaled dashed stroke and fractional-zoom rounding.
            int margin = (int)Math.Ceiling(SelectionBoxLineWidth / 2) + 1;
            Invalidate(ZoomClipRectangle(bounds, _zoomFactor, margin));
        }

        private void InvalidateRotationSelectionHandle()
        {
            if (!TryGetRotationHandle(out var handle)) return;
            float radius = 12 * RotationScreenScale;
            var owner = RotationHandleOwner;
            var top = RotationHandleAnchor(owner);
            var dirty = RectangleF.Union(new RectangleF(handle.X - radius, handle.Y - radius, radius * 2, radius * 2),
                new RectangleF(top.X - radius, top.Y - radius, radius * 2, radius * 2));
            InvalidateSelectionBox(Rectangle.Ceiling(dirty));
        }

        private void InvalidateSelectionMembers(IEnumerable<IDrawableContainer> members)
        {
            var changed = members.ToArray();
            foreach (var element in changed) InvalidateElements(element.DrawingBounds);
            foreach (var group in changed.OfType<DrawableContainer>().Where(element => element.GroupId != Guid.Empty).GroupBy(element => element.GroupId))
            {
                var bounds = group.Select(element => (Rectangle)element.Bounds).Aggregate(Rectangle.Union);
                InvalidateSelectionBox(bounds);
            }
        }

        private void PaintSelectionBox(Graphics graphics)
        {
            if (!_selectionBoxStart.HasValue || !_selectionBoxActive) return;
            var saved = graphics.Save();
            graphics.ScaleTransform(_zoomFactor, _zoomFactor);
            using var fill = new SolidBrush(Color.FromArgb(30, SystemColors.Highlight));
            using var pen = new Pen(SystemColors.Highlight, SelectionBoxLineWidth / (float)_zoomFactor) { DashStyle = DashStyle.Dash };
            graphics.FillRectangle(fill, SelectionBoxBounds);
            graphics.DrawRectangle(pen, SelectionBoxBounds);
            graphics.Restore(saved);
        }
    }
}
