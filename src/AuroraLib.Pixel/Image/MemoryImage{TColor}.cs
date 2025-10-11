using AuroraLib.Pixel.Metadata;
using AuroraLib.Pixel.Processing.Processor;
using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace AuroraLib.Pixel.Image
{
    /// <summary>
    /// Represents an image , providing span-based access to pixel data.
    /// </summary>
    /// <typeparam name="TColor">The pixel color type implementing <see cref="IColor{TColor}"/>.</typeparam>
    public sealed class MemoryImage<TColor> : IDirectRowAccess<TColor>
        where TColor : unmanaged, IColor<TColor>
    {
        private Memory<TColor> _pixelMemory;

        private readonly IDisposable? _disposable;

        /// <inheritdoc/>
        public int Width { get; private set; }
        /// <inheritdoc/>
        public int Height { get; private set; }

        /// <summary>
        /// Gets the number of pixels per row (stride).
        /// </summary>
        public int Stride { get; }

        /// <inheritdoc/>
        public ImageMetadata? Metadata { get; set; }

        /// <summary>
        /// Gets a span over the raw pixel data that can be modified directly.
        /// </summary>
        public Span<TColor> Pixel => _pixelMemory.Span;

        /// <inheritdoc/>
        public TColor this[int x, int y]
        {
            get
            {
                ThrowIfOutOfBounds(x, y);
                return Pixel[y * Stride + x];
            }
            set
            {
                ThrowIfOutOfBounds(x, y);
                Pixel[y * Stride + x] = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryImage{TColor}"/> class using the specified memory buffer and layout information.
        /// </summary>
        /// <param name="memory">The memory buffer containing the pixel data.</param>
        /// <param name="width">The width of the image, in pixels.</param>
        /// <param name="height">The height of the image, in pixels.</param>
        /// <param name="stride">The number of elements between the start of one row and the next. Must be greater than or equal to <paramref name="width"/>.
        public MemoryImage(Memory<TColor> memory, int width, int height, int stride)
        {
            if (memory.IsEmpty) throw new AggregateException(nameof(memory));
#if NET8_0_OR_GREATER
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
            ArgumentOutOfRangeException.ThrowIfLessThan(stride, width);
            ArgumentOutOfRangeException.ThrowIfLessThan(memory.Span.Length, stride * height);
#else
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
            if (stride < width) throw new ArgumentOutOfRangeException(nameof(stride));
            if (memory.Span.Length < stride * height) throw new ArgumentOutOfRangeException(nameof(memory));
#endif
            _pixelMemory = memory.Slice(0, stride * height);
            Width = width;
            Height = height;
            Stride = stride;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryImage{TColor}"/> class using a memory owner.
        /// </summary>
        /// <param name="memory">An <see cref="IMemoryOwner{TColor}"/> providing the memory buffer for the image.</param>
        /// <param name="width">The width of the image, in pixels.</param>
        /// <param name="height">The height of the image, in pixels.</param>
        /// <param name="stride">The number of elements between the start of one row and the start of the next.<paramref name="width"/>.
        /// </param>
        public MemoryImage(IMemoryOwner<TColor> memory, int width, int height, int stride) : this(memory.Memory, width, height, stride)
            => _disposable = memory;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryImage{TColor}"/> class.
        /// </summary>
        /// <param name="width">The width of the image, in pixels.</param>
        /// <param name="height">The height of the image, in pixels.</param>
        /// <param name="stride"> Optional stride (number of elements per row); if not specified or less than <paramref name="width"/>, defaults to <paramref name="width"/>.</param>
        public MemoryImage(int width, int height, int stride = default)
        {
#if NET8_0_OR_GREATER
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
#else
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
#endif
            if (stride < width)
                stride = NextPowerOfTwo(width);

            var pool = MemoryPool<TColor>.Shared.Rent(stride * height);
            _pixelMemory = pool.Memory;
            _disposable = pool;
            Width = width;
            Height = height;
            Stride = stride;
        }

        /// <inheritdoc/>
        public void Clear() => Pixel.Clear();

        /// <inheritdoc/>
        public void Crop(Rectangle region)
        {
            if (region.X < 0 || region.Y < 0 || region.X + region.Width > Width || region.Y + region.Height > Height)
                throw new ArgumentOutOfRangeException(nameof(region), "Crop region is outside the image bounds.");

            _pixelMemory = _pixelMemory.Slice(region.Y * Stride + region.X, Stride * region.Height);
            Width = region.Width;
            Height = region.Height;
        }

        /// <inheritdoc/>
        public IImage<TColor> Clone()
        {
            MemoryImage<TColor> clone = new MemoryImage<TColor>(Width, Height, Stride);
            Pixel.CopyTo(clone.Pixel);
            return clone;
        }

        IImage IReadOnlyImage.Clone() => Clone();

        IImage<TColor> IReadOnlyImage<TColor>.Create(int width, int height) => new MemoryImage<TColor>(width, height);

        /// <inheritdoc/>
        public void Apply(IPixelProcessor processor) => processor.Apply(this);

        /// <inheritdoc/>
        public void Apply(IReadOnlyPixelProcessor processor) => processor.Apply(this);

        /// <inheritdoc/>
        public ReadOnlySpan<TColor> GetRow(int y) => GetWritableRow(y);

        /// <inheritdoc/>
        public Span<TColor> GetWritableRow(int y) => Pixel.Slice(y * Stride, Width);

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_pixelMemory.IsEmpty)
            {
                _disposable?.Dispose();
                _pixelMemory = Memory<TColor>.Empty;
                Width = Height = 0;
            }
        }

        Vector4 IImage.this[int x, int y]
        {
            get => this[x, y].ToScaledVector4();
            set => Pixel[y * Stride + x].FromScaledVector4(value);
        }

        Vector4 IReadOnlyImage.this[int x, int y] => this[x, y].ToScaledVector4();

        TColor IReadOnlyImage<TColor>.this[int x, int y] => this[x, y];

        void IReadOnlyImage<TColor>.GetPixel(int x, int y, Span<TColor> pixelRow)
            => Pixel.Slice(y * Stride + x, pixelRow.Length).CopyTo(pixelRow);

        void IImage<TColor>.SetPixel(int x, int y, ReadOnlySpan<TColor> pixelRow)
            => pixelRow.CopyTo(Pixel.Slice(y * Stride + x, pixelRow.Length));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThrowIfOutOfBounds(int x, int y)
        {
            if ((uint)x >= Width)
                ThrowOutOfBoundsOfImage(x, Width, nameof(x), nameof(Width));
            if ((uint)y >= Height)
                ThrowOutOfBoundsOfImage(y, Height, nameof(y), nameof(Height));
        }

        private static int NextPowerOfTwo(int x) => (int)Math.Pow(2, Math.Ceiling(Math.Log(x, 2)));

        [MethodImpl(MethodImplOptions.NoInlining)]
#if NET6_0_OR_GREATER
        [DoesNotReturn]
        internal static void ThrowOutOfBoundsOfImage(int argument, int imageData, [CallerArgumentExpression(nameof(argument))] string? paramNameArgument = null, [CallerArgumentExpression(nameof(imageData))] string? paramNameImageData = null)
#else
        internal static void ThrowOutOfBoundsOfImage(int argument, int imageData, string paramNameArgument, string paramNameImageData)
#endif
            => throw new ArgumentOutOfRangeException(paramNameArgument, $"{paramNameArgument} = {argument} is outside the image {paramNameImageData} {imageData}.");

    }
}
