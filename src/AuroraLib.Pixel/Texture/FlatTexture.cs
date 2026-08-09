using AuroraLib.Pixel.Image;
using AuroraLib.Pixel.Metadata;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace AuroraLib.Pixel.Texture
{
    /// <summary>
    /// Represents a flat texture with a base level and mipmap level.
    /// </summary>
    public sealed class FlatTexture<TColor> : Texture<TColor> where TColor : unmanaged, IColor<TColor>
    {
        /// <summary>
        /// Gets the collection of all mipmap levels for this texture, where index 0 is the base level.
        /// </summary>
        public List<IImage<TColor>> Levels { get; }

        /// <inheritdoc/>
        public override int LevelCount => Levels.Count;

        public FlatTexture(IEnumerable<IImage<TColor>> levels)
        {
            Levels = new List<IImage<TColor>>(levels);
            if (Levels.Count == 0)
                throw new ArgumentException("A texture must contain at least one level.");
        }

        public FlatTexture(IImage<TColor> baseLevel)
            => Levels = new List<IImage<TColor>>() { baseLevel };

        /// <inheritdoc/>
        public override IImage<TColor> GetLevel(int index) => Levels[index];

        /// <inheritdoc/>
        public override void Clear()
        {
            Levels[0].Clear();
            for (int i = 1; i < Levels.Count; i++)
            {
                Levels[i].Dispose();
            }
            Levels.RemoveRange(1, Levels.Count - 1);
        }

        /// <inheritdoc/>
        public override IImage<TColor1> CloneAs<TColor1>(Rectangle region)
        {
            ImageMetadata? metadata = Metadata != null ? new ImageMetadata(Metadata) : null;
            return new FlatTexture<TColor1>(Levels.Select((level, index) => level.CloneAs<TColor1>(ScaleMipRegion(region, index)))) { Metadata = metadata };
        }

        private static Rectangle ScaleMipRegion(Rectangle region, int level)
        {
            int x = region.X >> level;
            int y = region.Y >> level;

            int width = Math.Max(1, region.Width >> level);
            int height = Math.Max(1, region.Height >> level);

            return new Rectangle(x, y, width, height);
        }
    }
}
