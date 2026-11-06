using System;
using System.Collections.Generic;
using System.Globalization;
using Greenshot.Editor.Drawing;
using Greenshot.Editor.Drawing.Fields;

namespace Greenshot.Editor.Measurements
{
    // One managed snapshot per drawable. No fonts, paths, bitmaps or other native resources survive layout.
    internal sealed class DimensionLayoutCache
    {
        private readonly object _parent;
        private readonly long _revision;
        private readonly DimensionStyle _style;
        private readonly MeasurementState _measurements;
        private readonly int _stroke;
        private readonly object _arrowheads;
        private readonly string _decimalSeparator, _negativeSign, _positiveSign;
        internal IReadOnlyList<DimensionLabel> Labels { get; }

        internal DimensionLayoutCache(DrawableContainer owner, DimensionLabel[] labels)
        {
            _parent = owner.Parent;
            _revision = owner.DimensionLayoutRevision;
            _style = (owner.Parent as Surface)?.DimensionStyle ?? DimensionStyle.Default;
            _measurements = (owner.Parent as Surface)?.Measurements ?? MeasurementState.Pixels;
            _stroke = owner.GetFieldValueAsInt(FieldType.LINE_THICKNESS);
            _arrowheads = owner is ArrowContainer ? owner.GetFieldValue(FieldType.ARROWHEADS) : null;
            var numbers = CultureInfo.CurrentCulture.NumberFormat;
            _decimalSeparator = numbers.NumberDecimalSeparator;
            _negativeSign = numbers.NegativeSign;
            _positiveSign = numbers.PositiveSign;
            Labels = Array.AsReadOnly(labels);
        }

        internal bool Matches(DrawableContainer owner)
        {
            var surface = owner.Parent as Surface;
            var numbers = CultureInfo.CurrentCulture.NumberFormat;
            return ReferenceEquals(_parent, owner.Parent) && _revision == owner.DimensionLayoutRevision &&
                ReferenceEquals(_style, surface?.DimensionStyle ?? DimensionStyle.Default) &&
                ReferenceEquals(_measurements, surface?.Measurements ?? MeasurementState.Pixels) &&
                _stroke == owner.GetFieldValueAsInt(FieldType.LINE_THICKNESS) &&
                Equals(_arrowheads, owner is ArrowContainer ? owner.GetFieldValue(FieldType.ARROWHEADS) : null) &&
                _decimalSeparator == numbers.NumberDecimalSeparator && _negativeSign == numbers.NegativeSign && _positiveSign == numbers.PositiveSign;
        }
    }
}
