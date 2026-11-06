using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Greenshot.Base.Core;
using Greenshot.Base.Interfaces.Drawing;
using Greenshot.Editor.Helpers;

namespace Greenshot.Editor.Drawing
{
    public sealed partial class Surface
    {
        private HashSet<IDrawableContainer> _snapExclusions;
        private readonly List<DrawableContainer> _snapStationary = new List<DrawableContainer>();
        private PointF[] _snapCachedTargets;
        private int _snapLockedTarget = -1, _snapLockedSource = -1;
        private IDrawableContainer _snapLockedOwner;
        private bool _snapLockedRay;
        private Keys _snapLockModifiers;
        private string _snapLockMode;
        private bool _snapGuideEditing;
        internal int SnapTargetBuildCount { get; private set; }

        private PointF[] SnapTargets(IEnumerable<IDrawableContainer> excluded)
        {
            var exclusions = new HashSet<IDrawableContainer>(excluded.Where(element => element != null));
            if (_snapCachedTargets != null && _snapExclusions != null && _snapExclusions.SetEquals(exclusions)) return _snapCachedTargets;
            ReleaseSnapTargets();
            _snapExclusions = exclusions;
            var targets = new List<PointF>();
            foreach (var element in _elements.Where(element => !exclusions.Contains(element)))
            {
                targets.AddRange(ObjectSnapHelper.Targets(element));
                if (element is DrawableContainer shape)
                {
                    shape.GeometryChanged += StationarySnapGeometryChanged;
                    _snapStationary.Add(shape);
                }
            }
            _snapCachedTargets = targets.ToArray();
            SnapTargetBuildCount++;
            return _snapCachedTargets;
        }

        private void ReleaseSnapTargets()
        {
            foreach (var shape in _snapStationary) shape.GeometryChanged -= StationarySnapGeometryChanged;
            _snapStationary.Clear();
            _snapCachedTargets = null;
            _snapExclusions = null;
            ClearSnapLock();
        }

        private void StationarySnapGeometryChanged(object sender, EventArgs args) => InvalidateSnapTargets();
        private void InvalidateSnapTargets()
        {
            _snapCachedTargets = null;
            ClearSnapLock();
            SetSnapGuide(null, null, null);
        }

        private void ClearSnapLock()
        {
            _snapLockedTarget = _snapLockedSource = -1;
            _snapLockedOwner = null;
            _snapLockMode = null;
        }

        private void PrepareSnapLock(string mode)
        {
            var modifiers = InputState.ModifierKeys;
            if (_snapLockMode != mode || modifiers != _snapLockModifiers) ClearSnapLock();
            _snapLockMode = mode;
            _snapLockModifiers = modifiers;
        }

        private void LockSnap(PointF? target, IDrawableContainer owner, int source, bool ray)
        {
            // The index identifies a feature in the immutable stationary snapshot; rebuilding it clears the lock.
            _snapLockedTarget = target.HasValue && _snapCachedTargets != null ? Array.IndexOf(_snapCachedTargets, target.Value) : -1;
            _snapLockedOwner = owner;
            _snapLockedSource = source;
            _snapLockedRay = ray;
        }

        private bool TryRetainedSnap(Point pointer, Point? anchor, IDrawableContainer owner, int source,
            bool points, bool through, Func<Point, bool> compatible, out Point end, out PointF? target, out bool ray)
        {
            end = pointer; target = null; ray = false;
            if (_snapLockedTarget < 0 || _snapCachedTargets == null || _snapLockedTarget >= _snapCachedTargets.Length ||
                !ReferenceEquals(owner, _snapLockedOwner) || source != _snapLockedSource) return false;
            // A real point always wins over a through-ray, even while retaining a ray lock.
            if (_snapLockedRay && points)
            {
                end = ObjectSnapHelper.Snap(pointer, anchor, _snapCachedTargets, true, false, _zoomFactor, compatible,
                    out target, out ray, _snappingRadius);
                if (target.HasValue) return true;
            }
            var candidate = _snapCachedTargets[_snapLockedTarget];
            end = ObjectSnapHelper.Snap(pointer, anchor, new[] { candidate }, !_snapLockedRay && points,
                _snapLockedRay && through, _zoomFactor, compatible, out target, out ray, _snappingRadius * 1.25);
            return target.HasValue;
        }

        private Rectangle SnapGuideBounds(PointF? target, Point? anchor, Point? end)
        {
            if (!target.HasValue) return Rectangle.Empty;
            float zoom = (float)(double)_zoomFactor;
            var bounds = new RectangleF(target.Value.X * zoom - 6, target.Value.Y * zoom - 6, 12, 12);
            if (anchor.HasValue && end.HasValue)
            {
                var line = RectangleF.FromLTRB(Math.Min(anchor.Value.X, end.Value.X) * zoom - 3,
                    Math.Min(anchor.Value.Y, end.Value.Y) * zoom - 3, Math.Max(anchor.Value.X, end.Value.X) * zoom + 3,
                    Math.Max(anchor.Value.Y, end.Value.Y) * zoom + 3);
                bounds = RectangleF.Union(bounds, line);
            }
            return Rectangle.Ceiling(bounds);
        }

        private void SetSnapGuide(PointF? target, Point? anchor, Point? end)
        {
            bool editing = target.HasValue && (_existingGestureAdorner != null || _vertexOwner != null || _mouseDownElement != null);
            if (_snapTarget == target && _snapAnchor == anchor && _snapEnd == end && _snapGuideEditing == editing) return;
            var old = SnapGuideBounds(_snapTarget, _snapAnchor, _snapEnd);
            var next = SnapGuideBounds(target, anchor, end);
            _snapTarget = target; _snapAnchor = anchor; _snapEnd = end;
            _snapGuideEditing = editing;
            if (!old.IsEmpty) Invalidate(old);
            if (!next.IsEmpty) Invalidate(next);
        }
    }
}
