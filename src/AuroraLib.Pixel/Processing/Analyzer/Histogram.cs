using AuroraLib.Pixel.PixelFormats;
using System;
using System.Buffers;

namespace AuroraLib.Pixel.Processing.Analyzer
{
    /// <summary>
    /// Represents the value distribution of the color channels in an image.
    /// </summary>
    public sealed class Histogram : IDisposable
    {
        private const int BinCount = 256;
        private readonly uint[] _data;
        private bool _disposed;

        public Histogram() => _data = ArrayPool<uint>.Shared.Rent(BinCount * 4);

        /// <summary>
        /// Gets the total number of pixels added to the histogram.
        /// </summary>
        public ulong Count { get; private set; }

        /// <summary>
        /// Gets the histogram values for the red channel.
        /// </summary>
        public ReadOnlySpan<uint> Red => _data.AsSpan(0, BinCount);

        /// <summary>
        /// Gets the histogram values for the green channel.
        /// </summary>
        public ReadOnlySpan<uint> Green => _data.AsSpan(BinCount, BinCount);

        /// <summary>
        /// Gets the histogram values for the blue channel.
        /// </summary>
        public ReadOnlySpan<uint> Blue => _data.AsSpan(BinCount * 2, BinCount);

        /// <summary>
        /// Gets the histogram values for the alpha channel.
        /// </summary>
        public ReadOnlySpan<uint> Alpha => _data.AsSpan(BinCount * 3, BinCount);

        public void Add<TColor>(ReadOnlySpan<TColor> pixels)
            where TColor : unmanaged, IColor<TColor>
        {
            RGBA<byte> rgba = default;

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i].ToRGBA(ref rgba);

                _data[rgba.R]++;
                _data[BinCount + rgba.G]++;
                _data[BinCount * 2 + rgba.B]++;
                _data[BinCount * 3 + rgba.A]++;
            }

            Count += (ulong)pixels.Length;
        }

        public void Reset()
        {
            _data.AsSpan(0, BinCount * 4).Clear();
            Count = 0;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            ArrayPool<uint>.Shared.Return(_data);
        }
    }
}
