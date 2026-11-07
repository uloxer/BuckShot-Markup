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

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel.Security;
using BuckShot.Base.Interfaces.Drawing;
using BuckShot.Editor.Drawing;
using BuckShot.Editor.Drawing.Emoji;
using BuckShot.Editor.Drawing.Fields;
using BuckShot.Editor.Drawing.Filters;
using log4net;
using static BuckShot.Editor.Drawing.FilterContainer;

namespace BuckShot.Editor.Helpers
{
    /// <summary>
    /// This helps to map the serialization of the old .greenshot file to the newer.
    /// It also prevents misuse.
    /// </summary>
    internal class BinaryFormatterHelper : SerializationBinder
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(BinaryFormatterHelper));
        private static readonly IDictionary<string, Type> TypeMapper = new Dictionary<string, Type>
        {
            {"System.Guid",typeof(Guid) },
            // Used specifically for the .ini configuration (besides the ones already defined)
            {"System.Int32",typeof(int) },
            {"System.Single",typeof(float) },
            {"System.Boolean",typeof(bool) },
            {"System.String",typeof(string) },
            // End ini configuration
            {"System.Drawing.Rectangle",typeof(System.Drawing.Rectangle) },
            {"System.Drawing.Point",typeof(System.Drawing.Point) },
            {"System.Drawing.Color",typeof(System.Drawing.Color) },
            {"System.Drawing.Bitmap",typeof(System.Drawing.Bitmap) },
            {"System.Drawing.Icon",typeof(System.Drawing.Icon) },
            {"System.Drawing.Imaging.Metafile",typeof(System.Drawing.Imaging.Metafile) },
            {"System.Drawing.Size",typeof(System.Drawing.Size) },
            {"System.IO.MemoryStream",typeof(System.IO.MemoryStream) },
            {"System.Drawing.StringAlignment",typeof(System.Drawing.StringAlignment) },
            {"System.Collections.Generic.List`1[[BuckShot.Base.Interfaces.Drawing.IFieldHolder", typeof(List<IFieldHolder>)},
            {"System.Collections.Generic.List`1[[BuckShot.Base.Interfaces.Drawing.IField", typeof(List<IField>)},
            {"System.Collections.Generic.List`1[[System.Drawing.Point", typeof(List<System.Drawing.Point>)},
            {"BuckShot.Editor.Drawing.ArrowContainer", typeof(ArrowContainer) },
            {"BuckShot.Editor.Drawing.ArrowContainer+ArrowHeadCombination", typeof(ArrowContainer.ArrowHeadCombination) },
            {"BuckShot.Editor.Drawing.LineContainer", typeof(LineContainer) },
            {"BuckShot.Editor.Drawing.TextContainer", typeof(TextContainer) },
            {"BuckShot.Editor.Drawing.SpeechbubbleContainer", typeof(SpeechbubbleContainer) },
            {"BuckShot.Editor.Drawing.RectangleContainer", typeof(RectangleContainer) },
            {"BuckShot.Editor.Drawing.CutMarkContainer", typeof(CutMarkContainer) },
            {"BuckShot.Editor.Drawing.TornEdgeContainer", typeof(TornEdgeContainer) },
            {"BuckShot.Editor.Drawing.EllipseContainer", typeof(EllipseContainer) },
            {"BuckShot.Editor.Drawing.FreehandContainer", typeof(FreehandContainer) },
            {"BuckShot.Editor.Drawing.PolygonContainer", typeof(PolygonContainer) },
            {"BuckShot.Editor.Drawing.HighlightContainer", typeof(HighlightContainer) },
            {"BuckShot.Editor.Drawing.IconContainer", typeof(IconContainer) },
            {"BuckShot.Editor.Drawing.ObfuscateContainer", typeof(ObfuscateContainer) },
            {"BuckShot.Editor.Drawing.StepLabelContainer", typeof(StepLabelContainer) },
            // Only used for the last used field values in the .ini
            {"BuckShot.Editor.Drawing.CutMarkStyle", typeof(CutMarkStyle) },
            {"BuckShot.Editor.Drawing.CropContainer+CropModes", typeof(CropContainer.CropModes) },
            {"BuckShot.Editor.Drawing.SvgContainer", typeof(SvgContainer) },
            {"BuckShot.Editor.Drawing.Emoji.EmojiContainer", typeof(EmojiContainer) },
            {"BuckShot.Editor.Drawing.VectorGraphicsContainer", typeof(VectorGraphicsContainer) },
            {"BuckShot.Editor.Drawing.MetafileContainer", typeof(MetafileContainer) },
            {"BuckShot.Editor.Drawing.ImageContainer", typeof(ImageContainer) },
            {"BuckShot.Editor.Drawing.FilterContainer", typeof(FilterContainer) },
            {"BuckShot.Editor.Drawing.DrawableContainer", typeof(DrawableContainer) },
            {"BuckShot.Editor.Drawing.DrawableContainerList", typeof(DrawableContainerList) },
            {"BuckShot.Editor.Drawing.CursorContainer", typeof(CursorContainer) },
            {"BuckShot.Editor.Drawing.CursorContainer+CaptureCursorSerializationWrapper", typeof(CursorContainer.CaptureCursorSerializationWrapper) },
            {"BuckShot.Editor.Drawing.Filters.HighlightFilter", typeof(HighlightFilter) },
            {"BuckShot.Editor.Drawing.Filters.GrayscaleFilter", typeof(GrayscaleFilter) },
            {"BuckShot.Editor.Drawing.Filters.MagnifierFilter", typeof(MagnifierFilter) },
            {"BuckShot.Editor.Drawing.Filters.BrightnessFilter", typeof(BrightnessFilter) },
            {"BuckShot.Editor.Drawing.Filters.BlurFilter", typeof(BlurFilter) },
            {"BuckShot.Editor.Drawing.Filters.PixelizationFilter", typeof(PixelizationFilter) },
            {"BuckShot.Base.Interfaces.Drawing.IDrawableContainer", typeof(IDrawableContainer) },
            {"BuckShot.Base.Interfaces.Drawing.EditStatus", typeof(EditStatus) },
            {"BuckShot.Base.Interfaces.Drawing.IFieldHolder", typeof(IFieldHolder) },
            {"BuckShot.Base.Interfaces.Drawing.IField", typeof(IField) },
            {"BuckShot.Base.Interfaces.Drawing.FieldFlag", typeof(FieldFlag) },
            {"BuckShot.Editor.Drawing.Fields.Field", typeof(Field) },
            {"BuckShot.Editor.Drawing.Fields.FieldType", typeof(FieldType) },
            {"BuckShot.Editor.Drawing.FilterContainer+PreparedFilter", typeof(PreparedFilter) },
        };

        /// <summary>
        /// Try to match the type for the given type name, this is used to check if the type is allowed to be deserialized, and to map old types to new ones.
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="type"></param>
        /// <returns>bool true if the mapping was possible</returns>
        public static bool TryGetType(string typeName, out Type type)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                type = null;
                return false;
            }
            // Historical project, clipboard and field-setting names resolve only through the
            // same strict allowlist as current names. Never use Type.GetType on untrusted input.
            string comparingTypeName = typeName
                .Replace("GreenshotPlugin.Interfaces.Drawing", "BuckShot.Base.Interfaces.Drawing")
                .Replace("Greenshot.Plugin.Drawing", "BuckShot.Base.Interfaces.Drawing")
                .Replace("Greenshot.Drawing", "BuckShot.Editor.Drawing")
                .Replace("Greenshot.", "BuckShot.");
            var typeNameCommaLocation = comparingTypeName.IndexOf(",");
            if (typeNameCommaLocation > 0)
            {
                comparingTypeName = comparingTypeName.Substring(0, typeNameCommaLocation);
            }

            // Correct wrong types (because of refactoring) to the correct ones, this is needed to load old .greenshot files
            comparingTypeName = comparingTypeName.Replace("BuckShot.Drawing", "BuckShot.Editor.Drawing");
            comparingTypeName = comparingTypeName.Replace("BuckShot.Plugin.Drawing", "BuckShot.Base.Interfaces.Drawing");
            comparingTypeName = comparingTypeName.Replace("BuckShotPlugin.Interfaces.Drawing", "BuckShot.Base.Interfaces.Drawing");
            comparingTypeName = comparingTypeName.Replace("BuckShot.Drawing.Fields", "BuckShot.Editor.Drawing.Fields");
            comparingTypeName = comparingTypeName.Replace("BuckShot.Drawing.Filters", "BuckShot.Editor.Drawing.Filters");
            return TypeMapper.TryGetValue(comparingTypeName, out type);
        }

        /// <summary>
        /// Do the type mapping
        /// </summary>
        /// <param name="assemblyName">Assembly for the type that was serialized</param>
        /// <param name="typeName">Type that was serialized</param>
        /// <returns>Type which was mapped</returns>
        /// <exception cref="SecurityAccessDeniedException">If something smells fishy</exception>
        public override Type BindToType(string assemblyName, string typeName)
        {
            if (TryGetType(typeName, out var returnType))
            {
                LOG.Info($"Mapped {assemblyName} - {typeName} to {returnType.FullName}");
                return returnType;
            }
            LOG.Warn($"Unexpected BuckShot type in .greenshot file detected, maybe vulnerability attack created with ysoserial? Suspicious type: {assemblyName} - {typeName}");
            throw new SecurityAccessDeniedException($"Suspicious type in .greenshot file: {assemblyName} - {typeName}");
        }
    }
}
