using AuroraLib.Pixel.Image;
using System;

namespace AuroraLib.Pixel.PixelProcessor
{
    /// <summary>
    /// Provides access to a single row of pixels in an image, either as a buffer or direct reference.
    /// </summary>
    /// <typeparam name="TColor">The color type of the pixels.</typeparam>
    public readonly ref struct RowAccessor<TColor> where TColor : unmanaged, IColor<TColor>
    {
        private readonly Span<TColor> BufferOrPixel;
        private readonly IImage<TColor> Image;
        private readonly int Width;
        private readonly int X;
        private readonly int Stride;

        public readonly bool IsBuffered => Stride == -1;

        public RowAccessor(IImage<TColor> image, int xOfset, int width, bool forceBuffering = false)
        {
            X = xOfset;
            Width = width;
            Image = image;
            if (!forceBuffering && image is IImageSpan<TColor> imageSpan)
            {
                Stride = imageSpan.Stride;
                BufferOrPixel = imageSpan.Pixel;
            }
            else
            {
                Stride = -1;
                BufferOrPixel = new TColor[width];
            }
        }

        public RowAccessor(IImage<TColor> image, int xOfset, Span<TColor> buffer)
        {
            X = xOfset;
            Width = buffer.Length;
            Image = image;
            Stride = -1;
            BufferOrPixel = buffer;
        }

        public Span<TColor> this[int y]
        {
            get
            {
                if (IsBuffered)
                {
                    Image.GetPixel(X, y, BufferOrPixel);
                    return BufferOrPixel;
                }
                else
                {
                    return BufferOrPixel.Slice(y * Stride + X, Width);
                }
            }
            set
            {
                if (IsBuffered)
                    Image.SetPixel(X, y, value);
                else
                    value.CopyTo(BufferOrPixel.Slice(y * Stride + X, Width));
            }
        }
    }
}
