using AuroraLib.Pixel.Image;
using System.Collections.Generic;
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
            => Levels = new List<IImage<TColor>>(levels);

        public FlatTexture(IImage<TColor> baseLevel)
            => Levels = new List<IImage<TColor>>() { baseLevel };

        /// <inheritdoc/>
        public override IImage<TColor> GetLevel(int index) => Levels[index];

        /// <inheritdoc/>
        public override IImage<TColor> Clone() => new FlatTexture<TColor>(Levels.Select(l => l.Clone()));

        /// <inheritdoc/>
        public override IImage<TColor> Create(int width, int height) => new FlatTexture<TColor>(Levels[0].Create(width, height));

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
    }
}
