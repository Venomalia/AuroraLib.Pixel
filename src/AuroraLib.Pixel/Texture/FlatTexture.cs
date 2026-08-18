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
        private readonly List<IImage<TColor>> _levels;

        /// <summary>
        /// Gets the collection of all mipmap levels for this texture, where index 0 is the base level.
        /// </summary>
        public IReadOnlyList<IImage<TColor>> Levels => _levels;

        /// <inheritdoc/>
        public override int LevelCount => _levels.Count;


        /// <inheritdoc/>
        public override int MipMapCount
        {
            get => _levels.Count - 1;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                if (MipMapCount == value)
                    return;

                if (value < MipMapCount)
                {
                    for (int i = _levels.Count - 1; i > value; i--)
                    {
                        _levels[i].Dispose();
                        _levels.RemoveAt(i);
                    }
                }
                else
                {
                    for (int i = _levels.Count; i < value + 1; i++)
                    {
                        var last = _levels[_levels.Count - 1];

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
                        _levels.Add(mip);
                    }
                }
            }
        }

        public FlatTexture(IEnumerable<IImage<TColor>> levels)
        {
            _levels = new List<IImage<TColor>>(levels);
            if (_levels.Count == 0)
                throw new ArgumentException("A texture must contain at least one level.");
        }

        public FlatTexture(IImage<TColor> baseLevel, int mipLevel = 0)
        {
            if (baseLevel is FlatTexture<TColor> tex)
                _levels = new List<IImage<TColor>>(tex._levels);
            else if (baseLevel is Texture<TColor>)
                throw new NotSupportedException();
            else
                _levels = new List<IImage<TColor>>() { baseLevel };

            MipMapCount = mipLevel;
        }

        /// <inheritdoc/>
        public override IImage<TColor> GetLevel(int index) => _levels[index];

        /// <inheritdoc/>
        public override void Clear()
        {
            _levels[0].Clear();
            for (int i = 1; i < _levels.Count; i++)
            {
                _levels[i].Dispose();
            }
            _levels.RemoveRange(1, _levels.Count - 1);
        }

        /// <inheritdoc/>
        public override IImage<TColor1> CloneAs<TColor1>(Rectangle region)
            => new FlatTexture<TColor1>(_levels.Select((level, index) => level.CloneAs<TColor1>(ScaleMipRegion(region, index))));

        /// <inheritdoc/>
        public override IImage Create(int width, int height) => new FlatTexture<TColor>((IImage<TColor>)_levels[0].Create(width, height));

        /// <summary>
        /// Adds the next mipmap level to the texture.
        /// The image must have the expected dimensions and use the same image type as the base level.
        /// </summary>
        /// <param name="map">The image to add as the next mipmap level.</param>
        public void Add(IImage<TColor> map)
        {
            int width = Width >> _levels.Count;
            int height = Height >> _levels.Count;

            if (map == null)
                throw new ArgumentNullException(nameof(map));

            if (width == 0 || height == 0)
                throw new InvalidOperationException("The texture has reached its maximum mipmap level.");

            if (map.Width != width || map.Height != height)
                throw new ArgumentException($"The mipmap must have a size of {width}x{height}.", nameof(map));

            if (map.GetType() != _levels[0].GetType())
                throw new ArgumentException("The mipmap must use the same image type as the base level.", nameof(map));

            if (map is IPaletteImage<TColor> pmap && _levels[0] is IPaletteImage<TColor> pbase)
            {
                pmap.Palette = pbase.Palette;
                pmap.Quantizer = new NearestPaletteColorPicker<TColor>();
            }
            _levels.Add(map);
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
