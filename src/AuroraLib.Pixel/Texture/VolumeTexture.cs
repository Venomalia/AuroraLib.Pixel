using AuroraLib.Pixel.Image;
using System;
using System.Collections.Generic;
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
        public override IImage<TColor> Clone() => new FlatTexture<TColor>(Depths.Select(l => l.Clone()));

        /// <inheritdoc/>
        public override IImage<TColor> Create(int width, int height) => new FlatTexture<TColor>(Depths[0].Create(width, height));
    }
}
