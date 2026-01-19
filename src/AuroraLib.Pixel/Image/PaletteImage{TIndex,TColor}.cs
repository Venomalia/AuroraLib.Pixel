using AuroraLib.Pixel.Metadata;
using AuroraLib.Pixel.PixelProcessor;
using AuroraLib.Pixel.PixelProcessor.Helper;
using AuroraLib.Pixel.Processing.Processor;
using AuroraLib.Pixel.Processing.Quantizer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;

namespace AuroraLib.Pixel.Image
{
    /// <summary>
    /// Represents a indexed image where each pixel is stored as an index of type <typeparamref name="TIndex"/> 
    /// into a modifiable color palette of type <typeparamref name="TColor"/>.
    /// </summary>
    /// <typeparam name="TIndex">The underlying type used to store palette indices in the image.</typeparam>
    /// <typeparam name="TColor">The actual color type used in the palette.</typeparam>
    public sealed class PaletteImage<TIndex, TColor> : IPaletteImage<TColor>
        where TIndex : unmanaged, IIndexColor, IColor<TIndex>
        where TColor : unmanaged, IColor<TColor>
    {
        private readonly IImage<TIndex> _image;
        private readonly TColor[] _palette;
        private readonly int[] _palette_ref;

        /// <inheritdoc/>
        public int Width => _image.Width;
        /// <inheritdoc/>
        public int Height => _image.Height;

        /// <inheritdoc/>
        public Span<TColor> Palette => _palette.AsSpan();

        ReadOnlySpan<TColor> IReadOnlyPaletteImage<TColor>.Palette => Palette;

        /// <inheritdoc/>
        public ReadOnlySpan<int> PaletteRefCounts => _palette_ref.AsSpan();

        /// <inheritdoc/>
        public ImageMetadata? Metadata { get; set; }

        /// <summary>
        /// Gets or sets the color quantizer used by this image. 
        /// The quantizer determines how new colors are added to the palette.
        /// </summary>
        public IColorQuantizer<TColor> Quantizer { get; set; } = new MergePaletteQuantizer<TColor>();

        /// <summary>
        /// Initializes a new <see cref="PaletteImage{TIndex, TColor}"/> using an indexed image and a palette.
        /// </summary>
        /// <param name="image">The indexed image containing palette indices of type <typeparamref name="TIndex"/>.</param>
        /// <param name="palette">The color palette. Each index in the image maps to a color in this span.</param>
        public PaletteImage(IImage<TIndex> image, ReadOnlySpan<TColor> palette) : this(image, palette.Length)
        => palette.CopyTo(_palette);

        /// <summary>
        /// Initializes a new <see cref="PaletteImage{TIndex, TColor}"/> using an indexed image and a requested palette size.
        /// </summary>
        /// <param name="image">The indexed image containing palette indices of type <typeparamref name="TIndex"/>.</param>
        /// <param name="requestedPaletteSize">The desired number of palette entries. Limited by the bit depth of <typeparamref name="TIndex"/>.</param>
        public PaletteImage(IImage<TIndex> image, int requestedPaletteSize = 2048) : this(image, requestedPaletteSize, false)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaletteImage{TIndex, TColor}"/> class with the specified dimensions.
        /// </summary>
        /// <param name="width">The width of the image in pixels.</param>
        /// <param name="height">The height of the image in pixels.</param>
        /// <param name="stride">The number of pixels per image row.</param>
        /// <param name="requestedPaletteSize">The desired number of palette entries. The actual size is limited by the bit depth of <typeparamref name="TIndex"/>.
        public PaletteImage(int width, int height, int stride = default, int requestedPaletteSize = 4095)
            : this(new MemoryImage<TIndex>(width, height, stride, true), requestedPaletteSize, true)
        { }

        internal PaletteImage(IImage<TIndex> image, ReadOnlySpan<TColor> palette, ReadOnlySpan<int> colorsUsed) : this(image, palette.Length, true)
        {
            palette.CopyTo(_palette);
            colorsUsed.CopyTo(_palette_ref);
        }

        internal PaletteImage(IImage<TIndex> image, int requestedPaletteSize, bool IsEmpty)
        {
#if NET8_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(image);
#else
            if (image is null) throw new ArgumentNullException(nameof(image));
#endif
            int maxPalletSize = 1 << default(TIndex).FormatInfo.BitsPerPixel;
            _image = image;
            _palette = new TColor[Math.Min(requestedPaletteSize, maxPalletSize)];
            _palette_ref = new int[_palette.Length];
            if (IsEmpty)
                _palette_ref[0] = _image.Height * _image.Width;
            else
                CalculateColorsUsed(image, _palette_ref);
        }

        /// <inheritdoc/>
        public TColor this[int x, int y]
        {
            get
            {
                int index = GetPixelIndex(x, y);
                return Palette[index];
            }
            set
            {
                TIndex v = default;
                v.I = GetOrAddColorIndex(value, GetPixelIndex(x, y));
                _image[x, y] = v;
            }
        }

        /// <inheritdoc/>
        public int GetPixelIndex(int x, int y) => _image[x, y].I;

        /// <inheritdoc/>
        public void SetPixelIndex(int x, int y, int index)
        {
            _palette_ref[index]++;
            _palette_ref[_image[x, y].I]--;

            TIndex value = default;
            value.I = index;
            _image[x, y] = value;
        }

        /// <inheritdoc/>
        public void GetPixel(int x, int y, Span<TColor> pixelRow)
        {
            if ((uint)x >= Width)
                throw new ArgumentOutOfRangeException(nameof(x));
            if ((uint)y >= Height)
                throw new ArgumentOutOfRangeException(nameof(y));

            int toCopy = Math.Min(pixelRow.Length, Width - x);
            ReadOnlySpan<TColor> palette = Palette;

            if (_image is IReadOnlyDirectRowAccess<TIndex> imageRowAccess)
            {
                ReadOnlySpan<TIndex> row = imageRowAccess.GetRow(y);
                for (int i = 0; i < toCopy; i++)
                {
                    pixelRow[i] = palette[row[x + i].I];
                }
            }
            else
            {
                Span<TIndex> pixel = toCopy <= 512 ? stackalloc TIndex[toCopy] : new TIndex[toCopy];
                _image.GetPixel(x, y, pixel);
                for (int i = 0; i < toCopy; i++)
                {
                    pixelRow[i] = palette[pixel[i].I];
                }
            }
        }

        /// <inheritdoc/>
        public void SetPixel(int x, int y, ReadOnlySpan<TColor> pixelRow)
        {
            if ((uint)x >= Width)
                throw new ArgumentOutOfRangeException(nameof(x));
            if ((uint)y >= Height)
                throw new ArgumentOutOfRangeException(nameof(y));

            int toCopy = Math.Min(pixelRow.Length, Width - x);

            var rowAccessor = new RowAccessor<TIndex>(_image, x, toCopy);
            Span<TIndex> row = rowAccessor[y];
            GetOrAddColorIndices(pixelRow.Slice(0, toCopy), row);

            if (rowAccessor.IsBuffered)
                rowAccessor[y] = row;
        }

        private int GetOrAddColorIndex(TColor newColor, int oldIndex)
        {
            var palett = _palette.AsSpan();
            _palette_ref[oldIndex]--;

            // If the color is not in the palette
            int index = palett.IndexOf(newColor);
            if (index < 0)
            {
                // Check if there is a free slot available
                index = _palette_ref.AsSpan().IndexOf(0);

                // If the palette is full, ask the quantizer which palette color will be replaced/merged.
                if (index < 0)
                    index = Quantizer.ResolveColor(this, newColor);
                else
                    // Add the new color to the palette
                    palett[index] = newColor;
            }
            _palette_ref[index]++;
            return index;
        }

        private void GetOrAddColorIndices(ReadOnlySpan<TColor> newColors, Span<TIndex> indices)
        {
            var palett = _palette.AsSpan();

            for (int i = 0; i < indices.Length; i++)
            {
                _palette_ref[indices[i].I]--;
            }

            var colorCounts = new Dictionary<TColor, int>(newColors.Length);
            for (int i = 0; i < newColors.Length; i++)
            {
                var color = newColors[i];
                colorCounts.TryGetValue(color, out int count);
                colorCounts[color] = count + 1;
            }

            foreach (var kv in colorCounts)
            {
                TColor color = kv.Key;
                int count = kv.Value;

                // If the color is not in the palette
                int index = palett.IndexOf(color);
                if (index < 0)
                {
                    // Check if there is a free slot available
                    index = _palette_ref.AsSpan().IndexOf(0);
                    if (index < 0)
                    {
                        // If the palette is full, ask the quantizer which palette color will be replaced/merged.
                        index = Quantizer.ResolveColor(this, color, count);
                    }
                    else
                    {
                        // Add the new color to the palette
                        palett[index] = color;
                    }
                }

                _palette_ref[index] += count;

                // Replaced count with index
                colorCounts[color] = index;
            }

            for (int i = 0; i < indices.Length; i++)
            {
                indices[i].I = colorCounts[newColors[i]];
            }
        }

        /// <inheritdoc/>
        public IReadOnlyImage GetBuffer() => _image;

        /// <inheritdoc/>
        public void Clear()
        {
            _image.Clear();
            _palette.AsSpan().Clear();
            _palette_ref.AsSpan().Clear();
            _palette_ref[0] = _image.Height * _image.Width;
        }

        /// <inheritdoc/>
        public void Crop(Rectangle region)
        {
            _image.Crop(region);
            CalculateColorsUsed(_image, _palette_ref);
        }

        /// <inheritdoc/>
        public IImage<TColor> Clone() => new PaletteImage<TIndex, TColor>(_image.Clone(), _palette, _palette_ref);

        IImage IReadOnlyImage.Clone() => Clone();

        IImage<TColor> IReadOnlyImage<TColor>.Create(int width, int height) => new PaletteImage<TIndex, TColor>(_image.Create(width, height), _palette.Length, true);

        /// <inheritdoc/>
        public void Apply(IPixelProcessor processor) => processor.Apply(this);

        /// <inheritdoc/>
        public void Apply(IReadOnlyPixelProcessor processor) => processor.Apply(this);

        /// <inheritdoc/>
        public void Dispose()
            => _image?.Dispose();

        Vector4 IImage.this[int x, int y]
        {
            get => this[x, y].ToScaledVector4();
            set
            {
                TColor color = default;
                color.FromScaledVector4(value);
                this[x, y] = color;
            }
        }
        Vector4 IReadOnlyImage.this[int x, int y] => this[x, y].ToScaledVector4();
        TColor IReadOnlyImage<TColor>.this[int x, int y] => this[x, y];

        /// <inheritdoc/>
        public void ReplaceColor(int oldIndex, int newIndex)
        {
            var rowAccessor = new RowAccessor<TIndex>(_image, 0, _image.Width);
            int remaining = _palette_ref[oldIndex]; ;

            for (int y = 0; y < _image.Height; y++)
            {
                Span<TIndex> row = rowAccessor[y];
                for (int x = 0; x < _image.Width; x++)
                {
                    if (row[x].I == oldIndex)
                    {
                        row[x].I = newIndex;
                        remaining--;
                    }
                }
                if (rowAccessor.IsBuffered)
                    rowAccessor[y] = row;

                if (remaining == 0)
                {
                    _palette_ref[newIndex] += _palette_ref[oldIndex];
                    _palette_ref[oldIndex] = 0;
                    return;
                }
            }
            // If we land here, something is wrong.
            Debug.Fail("ReplaceIndex finished but oldIndex is still in use.");
            CalculateColorsUsed(_image, _palette_ref);
        }

        private static void CalculateColorsUsed(IReadOnlyImage<TIndex> image, Span<int> colorsUsed)
        {
            colorsUsed.Clear();
            var rowAccessor = new ReadOnlyRowAccessor<TIndex>(image, 0, image.Width);

            for (int y = 0; y < image.Height; y++)
            {
                ReadOnlySpan<TIndex> row = rowAccessor[y];
                for (int x = 0; x < image.Width; x++)
                {
                    colorsUsed[row[x].I]++;
                }
            }
        }
    }
}
