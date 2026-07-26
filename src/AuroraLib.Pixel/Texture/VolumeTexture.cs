using AuroraLib.Pixel.Image;
using AuroraLib.Pixel.Processing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace AuroraLib.Pixel.Texture
{
    /// <summary>
    /// Represents a 3D volume texture composed of multiple depth slices.
    /// </summary>
    public sealed class VolumeTexture<TColor> : Texture<TColor> where TColor : unmanaged, IColor<TColor>
    {
        /// <summary>
        /// Gets the collection of depth slices that make up this volume texture.
        /// </summary>
        public List<FlatTexture<TColor>> Depths { get; }

        /// <inheritdoc/>
        public override int LevelCount => Depths.Count;

        public VolumeTexture(IEnumerable<FlatTexture<TColor>> depths)
            => Depths = new List<FlatTexture<TColor>>(depths);

        public VolumeTexture(FlatTexture<TColor> baseDepth)
            => Depths = new List<FlatTexture<TColor>>() { baseDepth };

        /// <inheritdoc/>
        public override IImage<TColor> GetLevel(int index) => Depths[index];

        /// <inheritdoc/>
        public override IImage<TColor1> CloneAs<TColor1>(Rectangle region)
            => new FlatTexture<TColor1>(Depths.Select(l => l.CloneAs<TColor1>(region)));
    }
}
