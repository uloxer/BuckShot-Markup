using System;
using System.Drawing;
using BuckShot.Editor.Measurements;

namespace BuckShot.Editor.Configuration
{
    /// <summary>Fresh-document appearance only; project metadata restores its own style after construction.</summary>
    internal static class MeasurementDefaults
    {
        internal static DimensionStyle Create(IEditorConfiguration config)
        {
            if (config == null) return DimensionStyle.Default;
            var mode = config.MeasurementPrecisionMode;
            var format = new DimensionFormat(mode, mode == DimensionFormatMode.Automatic ? 0 :
                Math.Max(mode == DimensionFormatMode.SignificantFigures ? 1 : 0, config.MeasurementPrecisionDigits));
            return new DimensionStyle(config.MeasurementFontFamily, config.MeasurementFontSize, config.MeasurementFontStyle,
                config.MeasurementFontColor, format, Color.FromArgb(config.MeasurementBackgroundTransparent ? 0 : 255, config.MeasurementBackgroundColor));
        }
    }
}
