/*
 * Greenshot - a free and open source screenshot tool
 * Copyright (C) 2007-2026 Thomas Braun, Jens Klingen, Robin Krom
 *
 * For more information see: https://getgreenshot.org/
 * The Greenshot project is hosted on GitHub https://github.com/greenshot/greenshot
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using System.Drawing;

namespace BuckShot.Editor.Configuration
{
    public partial class EditorConfigurationImpl : IEditorConfiguration
    {
        public EditorConfigurationImpl()
        {
            RecentColors = new List<Color>();
            RecentEmojis = new List<string>();
        }

        partial void OnRecentColorsGet(ref List<Color> value)
        {
            if (value == null)
            {
                _recentColors = value = new List<Color>();
            }
        }

        partial void OnRecentEmojisGet(ref List<string> value)
        {
            if (value == null)
            {
                _recentEmojis = value = new List<string>();
            }
        }

        /// <summary>
        /// This should have been Math.Clamp, but it's not available for .NET Framework 4.8
        /// </summary>
        /// <param name="value">The value to clamp</param>
        /// <param name="min">The minimum value</param>
        /// <param name="max">The maximum value</param>
        /// <returns>The clamped value</returns>
        private static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        partial void OnFreehandSensitivitySet(ref int value) => value = Clamp(value, 1, 100);

        public void OnAfterLoad()
        {
            RecentColors ??= new List<Color>();
            RecentEmojis ??= new List<string>();
        }

        partial void OnSnappingRadiusSet(ref int value) => value = Clamp(value, 1, 100);
        partial void OnMeasurementPrecisionDigitsSet(ref int value) => value = Clamp(value, 0, 10);
        partial void OnMeasurementFontSizeSet(ref float value) => value = float.IsNaN(value) || float.IsInfinity(value) ? 11 : System.Math.Max(1, System.Math.Min(144, value));
        partial void OnMeasurementFontFamilySet(ref string value) { if (string.IsNullOrWhiteSpace(value) || value.Length > 128) value = "Arial"; }
        partial void OnMeasurementFontStyleSet(ref FontStyle value) { if (((int)value & ~15) != 0) value = FontStyle.Regular; }
        partial void OnMeasurementPrecisionModeSet(ref BuckShot.Editor.Measurements.DimensionFormatMode value)
        { if (value < BuckShot.Editor.Measurements.DimensionFormatMode.Automatic || value > BuckShot.Editor.Measurements.DimensionFormatMode.SignificantFigures) value = BuckShot.Editor.Measurements.DimensionFormatMode.Automatic; }
    }
}
