using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Greenshot.Base.Interfaces.Drawing;

namespace Greenshot.Editor.Drawing
{
    public sealed partial class Surface
    {
        private readonly Dictionary<DrawableContainer, Rectangle> _elementVisualSnapshots = new Dictionary<DrawableContainer, Rectangle>();
        private readonly Dictionary<Guid, Rectangle> _groupVisualSnapshots = new Dictionary<Guid, Rectangle>();

        internal bool TracksElementVisual(DrawableContainer shape) =>
            _elementVisualSnapshots.ContainsKey(shape) || ReferenceEquals(RotationHandleOwner, shape);

        private Rectangle ElementVisualBounds(DrawableContainer shape)
        {
            var bounds = (Rectangle)ToSurfaceCoordinates(shape.DrawingBounds);
            int handleMargin = shape.Adorners.Count == 0 ? 2 : shape.Adorners.Max(adorner => Math.Max(adorner.Bounds.Width, adorner.Bounds.Height)) / 2 + 3;
            bounds.Inflate(handleMargin, handleMargin);
            if (shape is PolygonContainer polygon && selectedElements.Count == 1 && ReferenceEquals(selectedElements[0], polygon))
            {
                var text = TextRenderer.MeasureText(polygon.ImageVertices.Length.ToString(), SystemFonts.DefaultFont);
                int vertexMargin = (int)Math.Ceiling((Math.Max(text.Width, text.Height) + 8) * (double)_zoomFactor) + 3;
                bounds.Inflate(vertexMargin, vertexMargin);
            }
            if (ReferenceEquals(RotationHandleOwner, shape) && TryGetRotationHandle(out var handle))
            {
                var top = RotationHandleAnchor(shape);
                float zoom = (float)(double)_zoomFactor;
                float radius = 12 * CurrentDpi / 96f;
                var stem = RectangleF.FromLTRB(Math.Min(top.X, handle.X) * zoom - radius,
                    Math.Min(top.Y, handle.Y) * zoom - radius, Math.Max(top.X, handle.X) * zoom + radius,
                    Math.Max(top.Y, handle.Y) * zoom + radius);
                bounds = Rectangle.Union(bounds, Rectangle.Ceiling(stem));
            }
            return bounds;
        }

        private Rectangle GroupVisualBounds(Guid groupId)
        {
            var members = selectedElements.OfType<DrawableContainer>().Where(shape => shape.GroupId == groupId).ToArray();
            return GroupVisualBounds(members);
        }

        private Rectangle GroupVisualBounds(DrawableContainer[] members)
        {
            if (members.Length < 2) return Rectangle.Empty;
            var bounds = members.Select(shape => (Rectangle)shape.Bounds).Aggregate(Rectangle.Union);
            var device = (Rectangle)ToSurfaceCoordinates(bounds);
            device.Inflate(3, 3);
            return device;
        }

        private void RememberElementVisual(DrawableContainer shape)
        {
            _elementVisualSnapshots[shape] = ElementVisualBounds(shape);
            if (shape.GroupId != Guid.Empty) _groupVisualSnapshots[shape.GroupId] = GroupVisualBounds(shape.GroupId);
        }

        private void RememberSelectionVisuals()
        {
            foreach (var shape in selectedElements.OfType<DrawableContainer>())
                _elementVisualSnapshots[shape] = ElementVisualBounds(shape);
            foreach (var group in selectedElements.OfType<DrawableContainer>().Where(shape => shape.GroupId != Guid.Empty).GroupBy(shape => shape.GroupId))
                _groupVisualSnapshots[group.Key] = GroupVisualBounds(group.ToArray());
        }

        internal void InvalidateElementVisual(DrawableContainer shape)
        {
            var current = ElementVisualBounds(shape);
            bool known = _elementVisualSnapshots.TryGetValue(shape, out var previous);
            var dirty = known ? Rectangle.Union(previous, current) : current;
            if (shape.GroupId != Guid.Empty)
            {
                var outline = GroupVisualBounds(shape.GroupId);
                bool groupKnown = _groupVisualSnapshots.TryGetValue(shape.GroupId, out var oldOutline);
                if (!outline.IsEmpty) dirty = Rectangle.Union(dirty, outline);
                if (groupKnown && !oldOutline.IsEmpty) dirty = Rectangle.Union(dirty, oldOutline);
                if (!outline.IsEmpty && !groupKnown) known = false;
                _groupVisualSnapshots[shape.GroupId] = outline;
            }
            _elementVisualSnapshots[shape] = current;
            // Unknown previous pixels and inverted/document-wide effects retain the full repaint fallback.
            if (!known || shape.Filters.Any(filter => filter.Invert)) Invalidate();
            else Invalidate(dirty);
        }

        private void ForgetElementVisual(DrawableContainer shape)
        {
            _elementVisualSnapshots.Remove(shape);
            if (shape.GroupId != Guid.Empty && !_elements.OfType<DrawableContainer>().Any(member => member.GroupId == shape.GroupId))
                _groupVisualSnapshots.Remove(shape.GroupId);
        }
    }
}
