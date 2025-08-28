using AuroraLib.Pixel.PixelProcessor;
using AuroraLib.Pixel.PixelProcessor.Helper;
using AuroraLib.Pixel.Processing.Processor;
using System;
using System.Buffers;
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

        /// <inheritdoc/>
        public int Width => _image.Width;
        /// <inheritdoc/>
        public int Height => _image.Height;
        /// <inheritdoc/>
        public int ColorsUsed { get; private set; }

        /// <inheritdoc/>
        public Span<TColor> Palette => _palette.AsSpan();

        ReadOnlySpan<TColor> IReadOnlyPaletteImage<TColor>.Palette => Palette;

        /// <summary>
        /// Initializes a new <see cref="PaletteImage{TIndex, TColor}"/> using an indexed image and a palette.
        /// </summary>
        /// <param name="image">The indexed image containing palette indices of type <typeparamref name="TIndex"/>.</param>
        /// <param name="palette">The color palette. Each index in the image maps to a color in this span.</param>
        /// <param name="requestedPaletteSize">The desired number of palette entries. Limited by the bit depth of <typeparamref name="TIndex"/>.</param>
        public PaletteImage(IImage<TIndex> image, ReadOnlySpan<TColor> palette, int requestedPaletteSize = 2048) : this(image, CalculateHighestUsedPallet(image), palette, Math.Max(palette.Length, requestedPaletteSize))
        { }

        /// <summary>
        /// Initializes a new <see cref="PaletteImage{TIndex, TColor}"/> using an indexed image, a palette, and the number of distinct colors used.
        /// </summary>
        /// <param name="image">The indexed image containing palette indices of type <typeparamref name="TIndex"/>.</param>
        /// <param name="colorsUsed">The number of distinct palette indices used in the image.</param>
        /// <param name="palette">The color palette. Each index in the image maps to a color in this span.</param>
        /// <param name="requestedPaletteSize">The desired number of palette entries. Limited by the bit depth of <typeparamref name="TIndex"/>.</param>
        public PaletteImage(IImage<TIndex> image, int colorsUsed, ReadOnlySpan<TColor> palette, int requestedPaletteSize = 2048) : this(image, colorsUsed, Math.Max(palette.Length, requestedPaletteSize))
            => palette.CopyTo(_palette);

        /// <summary>
        /// Initializes a new <see cref="PaletteImage{TIndex, TColor}"/> using an indexed image and a requested palette size.
        /// </summary>
        /// <param name="image">The indexed image containing palette indices of type <typeparamref name="TIndex"/>.</param>
        /// <param name="colorsUsed">The number of distinct palette indices used in the image.</param>
        /// <param name="requestedPaletteSize">The desired number of palette entries. Limited by the bit depth of <typeparamref name="TIndex"/>.</param>
        public PaletteImage(IImage<TIndex> image, int colorsUsed, int requestedPaletteSize)
        {
#if NET8_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(image);
#else
            if (image is null) throw new ArgumentNullException(nameof(image));
#endif
            int maxPalletSize = 1 << default(TIndex).FormatInfo.BitsPerPixel;

            _image = image;
            _palette = new TColor[Math.Min(Math.Max(requestedPaletteSize, colorsUsed), maxPalletSize)];
            ColorsUsed = colorsUsed;
        }

        /// <inheritdoc cref="PaletteImage{TIndex, TColor}.PaletteImage(IImage{TIndex}, int, int)"/>
        public PaletteImage(IImage<TIndex> image, int requestedPaletteSize = 2048) : this(image, CalculateHighestUsedPallet(image), requestedPaletteSize)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaletteImage{TIndex, TColor}"/> class with the specified dimensions.
        /// </summary>
        /// <param name="width">The width of the image in pixels.</param>
        /// <param name="height">The height of the image in pixels.</param>
        /// <param name="stride">The number of pixels per image row.</param>
        /// <param name="requestedPaletteSize">The desired number of palette entries. The actual size is limited by the bit depth of <typeparamref name="TIndex"/>.
        public PaletteImage(int width, int height, int stride = default, int requestedPaletteSize = 4095)
            : this(new MemoryImage<TIndex>(width, height, stride), 1, requestedPaletteSize)
        { }

        private static int CalculateHighestUsedPallet(IReadOnlyImage<TIndex> image)
        {
            int highestUsedPallet = 0;
            int maxPallet = (1 << default(TIndex).FormatInfo.BitsPerPixel);
            var rowAccessor = new ReadOnlyRowAccessor<TIndex>(image, 0, image.Width);

            for (int y = 0; y < image.Height; y++)
            {
                ReadOnlySpan<TIndex> row = rowAccessor[y];
                for (int x = 0; x < image.Width; x++)
                {
                    if (row[x].I > highestUsedPallet)
                    {
                        highestUsedPallet = row[x].I;
                        if (highestUsedPallet + 1 == maxPallet)
                            return maxPallet;
                    }
                }
            }

            return highestUsedPallet + 1;
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
                int index = GetColorIndexOrAdd(value);
                SetPixelIndex(x, y, index);
            }
        }

        /// <inheritdoc/>
        public int GetPixelIndex(int x, int y) => _image[x, y].I;

        /// <inheritdoc/>
        public void SetPixelIndex(int x, int y, int index)
        {
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

            if (_image is IDirectRowAccess<TIndex> imageRowAccess)
            {
                Span<TIndex> row = imageRowAccess.GetWritableRow(y);
                for (int i = 0; i < toCopy; i++)
                {
                    row[x + i].I = GetColorIndexOrAdd(pixelRow[i]);
                }
            }
            else
            {
                Span<TIndex> pixel = toCopy <= 512 ? stackalloc TIndex[toCopy] : new TIndex[toCopy];
                for (int i = 0; i < toCopy; i++)
                {
                    pixel[i].I = GetColorIndexOrAdd(pixelRow[i]);
                }
                _image.SetPixel(x, y, pixel);
            }
        }


        private int GetColorIndexOrAdd(TColor color)
        {
            int index = _palette.AsSpan(0, ColorsUsed).IndexOf(color);

            // If the color is not in the palette
            if (index < 0)
            {
                index = ColorsUsed;

                // If the palette is full, find the two most similar colors, blend them, and update the palette
                if (index >= _palette.Length)
                    return MergeMostSimilarColor(color);

                // Add the new color to the palette
                _palette[index] = color;
                ColorsUsed++;
            }

            return index;
        }

        /// <inheritdoc/>
        public IImage GetBuffer() => _image;

        /// <inheritdoc/>
        public void Clear()
        {
            _image.Clear();
            Palette[0] = default;
            ColorsUsed = default(TIndex).I + 1;
        }

        /// <inheritdoc/>
        public void Crop(Rectangle region)
        {
            _image.Crop(region);
            ColorsUsed = CalculateHighestUsedPallet(_image);
        }

        /// <inheritdoc/>
        public IImage<TColor> Clone() => new PaletteImage<TIndex, TColor>(_image.Clone(), ColorsUsed, Palette);

        IImage IReadOnlyImage.Clone() => Clone();

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

        private int MergeMostSimilarColor(TColor newColor)
        {
            Span<TColor> palette = Palette;
            // Convert palette colors to Vector4
            using IMemoryOwner<Vector4> memoryBuffer = MemoryPool<Vector4>.Shared.Rent(palette.Length + 1);
            Span<Vector4> buffer = memoryBuffer.Memory.Span.Slice(0, palette.Length + 1);
            for (int i = 0; i < palette.Length; i++)
            {
                buffer[i] = palette[i].ToScaledVector4();
            }
            buffer[palette.Length] = newColor.ToScaledVector4();


            // Find the two most similar colors in the palette
            (int color1, int color2) = FindMostSimilarColors(buffer);
            palette[color1].FromScaledVector4(Vector4.Lerp(buffer[color1], buffer[color2], 0.5f));

            // If the second color isn't the new one, replace it in the image
            if (color2 != palette.Length)
            {
                Replace(_image, color2, color1);
                palette[color2] = newColor;
                return color2;
            }
            return color1;
        }

        private static void Replace(IImage<TIndex> image, int oldIndex, int newIndex)
        {
            var rowAccessor = new RowAccessor<TIndex>(image, 0, image.Width);

            for (int y = 0; y < image.Height; y++)
            {
                Span<TIndex> row = rowAccessor[y];
                for (int x = 0; x < image.Width; x++)
                {
                    if (row[x].I == oldIndex)
                    {
                        row[x].I = newIndex;
                    }
                }
                if (rowAccessor.IsBuffered)
                    rowAccessor[y] = row;
            }
        }

        private static (int color1, int color2) FindMostSimilarColors(ReadOnlySpan<Vector4> palette)
        {
            float minDistance = float.MaxValue;
            int color1 = default, color2 = default;

            for (int i = 0; i < palette.Length; i++)
            {
                for (int j = i + 1; j < palette.Length; j++)
                {
                    float distance = CalculateColorDistance(palette[i], palette[j]);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        color1 = i;
                        color2 = j;
                    }
                }
            }

            return (color1, color2);
            static float CalculateColorDistance(Vector4 color1, Vector4 color2)
            {
                Vector4 Difference = Vector4.Abs(color1 - color2);
                return Difference.Length() * (1f + Difference.W);
            }
        }
    }
}
