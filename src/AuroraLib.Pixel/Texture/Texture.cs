using AuroraLib.Pixel.Image;
using AuroraLib.Pixel.Metadata;
using AuroraLib.Pixel.Processing.Processor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace AuroraLib.Pixel.Texture
{
    /// <summary>
    /// Represents a multi-level texture containing one or more image levels, such as mipmaps, animation frames, or layers.
    /// </summary>
    /// <typeparam name="TColor">The pixel color type.</typeparam>
    public abstract class Texture<TColor> : IImage<TColor>, IEnumerable<IImage<TColor>> where TColor : unmanaged, IColor<TColor>
    {
        /// <summary>
        /// Gets the total number of levels in this texture, including the base level.
        /// </summary>
        public abstract int LevelCount { get; }

        /// <summary>
        /// Retrieves the specified level from this texture. Level 0 represents the base level.
        /// </summary>
        /// <param name="index">The zero-based mipmap level index to retrieve.</param>
        /// <returns>The <see cref="IImage{TColor}"/> instance representing the requested level.</returns>
        public abstract IImage<TColor> GetLevel(int index);

        private IImage<TColor> BaseLevel
        {
            get
            {
                if (LevelCount < 1)
                    throw new InvalidOperationException("Base level is not set.");
                return GetLevel(0);
            }
        }

        /// <inheritdoc/>
        public int Width => BaseLevel.Width;

        /// <inheritdoc/>
        public int Height => BaseLevel.Height;

        /// <inheritdoc/>
        public ImageMetadata? Metadata { get => BaseLevel.Metadata; set => BaseLevel.Metadata = value; }

        /// <inheritdoc/>
        public TColor this[int x, int y]
        {
            get => BaseLevel[x, y];
            set => BaseLevel[x, y] = value;
        }

        Vector4 IImage.this[int x, int y]
        {
            get => ((IImage)BaseLevel)[x, y];
            set => ((IImage)BaseLevel)[x, y] = value;
        }
        Vector4 IReadOnlyImage.this[int x, int y] => ((IReadOnlyImage)BaseLevel)[x, y];

        void IImage.Apply(IPixelProcessor processor)
        {
            int baseWidth = BaseLevel.Width, baseHeight = BaseLevel.Height;
            foreach (var level in this)
            {
                if (baseWidth == level.Width && baseHeight == level.Height)
                    level.Apply(processor);
            }
        }

        void IReadOnlyImage.Apply(IReadOnlyPixelProcessor processor)
            => BaseLevel.Apply(processor);

        void IReadOnlyImage<TColor>.GetPixel(int x, int y, Span<TColor> pixelRow) => BaseLevel.GetPixel(x, y, pixelRow);
        void IImage<TColor>.SetPixel(int x, int y, ReadOnlySpan<TColor> pixelRow) => BaseLevel.SetPixel(x, y, pixelRow);
        IImage IReadOnlyImage.Clone() => Clone();

        /// <inheritdoc/>
        public abstract IImage<TColor> Clone();

        /// <inheritdoc/>
        public abstract IImage<TColor> Create(int width, int height);

        /// <inheritdoc/>
        public void Crop(Rectangle region)
        {
            int baseWidth = BaseLevel.Width, baseHeight = BaseLevel.Height;
            BaseLevel.Crop(region);

            for (int i = 1; i < LevelCount; i++)
            {
                var level = GetLevel(i);
                int levelWidth = level.Width;
                int levelHeight = level.Height;

                int x = region.X * levelWidth / baseWidth;
                int y = region.Y * levelHeight / baseHeight;
                int w = region.Width * levelWidth / baseWidth;
                int h = region.Height * levelHeight / baseHeight;

                // Clamp to ensure we don't exceed the level boundaries
                w = Math.Min(w, levelWidth - x);
                h = Math.Min(h, levelHeight - y);
                level.Crop(new Rectangle(x, y, w, h));
            }
        }
        /// <inheritdoc/>
        public virtual void Clear()
        {
            foreach (var level in this)
                level.Clear();
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            foreach (var level in this)
                level.Dispose();
        }

        IEnumerator<IImage<TColor>> IEnumerable<IImage<TColor>>.GetEnumerator()
        {
            for (int i = 0; i < LevelCount; i++)
                yield return GetLevel(i);
        }

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<IImage<TColor>>)this).GetEnumerator();
    }
}
