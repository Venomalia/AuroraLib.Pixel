using AuroraLib.Pixel.Image;
using AuroraLib.Pixel.Processing;
using AuroraLib.Pixel.Processing.Quantizer;
using AuroraLib.Pixel.Processing.Resampler;
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


        /// <inheritdoc/>
        public override int MipMapCount
        {
            get => Levels.Count - 1;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                if (MipMapCount == value)
                    return;

                if (value < MipMapCount)
                {
                    for (int i = Levels.Count - 1; i > value; i--)
                    {
                        Levels[i].Dispose();
                        Levels.RemoveAt(i);
                    }
                }
                else
                {
                    for (int i = Levels.Count; i < value + 1; i++)
                    {
                        var last = Levels[Levels.Count - 1];

                        if (last.Width == 1 || last.Height == 1)
                            break;

                        int width = last.Width >> 1;
                        int height = last.Height >> 1;

                        var mip = (IImage<TColor>)last.Create(width, height);

                        // Reuse the same palette for all mipmap levels and prevent palette changes.
                        if (last is IPaletteImage<TColor> pi && mip is IPaletteImage<TColor> target)
                        {
                            target.Palette = pi.Palette;
                            target.Quantizer = new NearestPaletteColorPicker<TColor>();
                        }

                        mip.ResizeFrom(last, Resamplers.Box);
                        Levels.Add(mip);
                    }
                }
            }
        }

        public FlatTexture(IEnumerable<IImage<TColor>> levels)
        {
            Levels = new List<IImage<TColor>>(levels);
            if (Levels.Count == 0)
                throw new ArgumentException("A texture must contain at least one level.");
        }

        public FlatTexture(IImage<TColor> baseLevel, int mipLevel = 0)
        {
            Levels = new List<IImage<TColor>>() { baseLevel };
            MipMapCount = mipLevel;
        }

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
            => new FlatTexture<TColor1>(Levels.Select((level, index) => level.CloneAs<TColor1>(ScaleMipRegion(region, index))));

        /// <inheritdoc/>
        public override IImage Create(int width, int height) => new FlatTexture<TColor>((IImage<TColor>)Levels[0].Create(width, height));

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
