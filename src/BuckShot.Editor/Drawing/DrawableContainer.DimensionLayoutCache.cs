using System;
using BuckShot.Editor.Measurements;

namespace BuckShot.Editor.Drawing
{
    public abstract partial class DrawableContainer
    {
        [NonSerialized] internal DimensionLayoutCache DimensionLayoutCache;
        [NonSerialized] internal long DimensionLayoutRevision;
        [NonSerialized] internal int DimensionLayoutBuildCount;

        private void InvalidateDimensionLayout()
        {
            DimensionLayoutRevision++;
            DimensionLayoutCache = null;
        }
    }
}
