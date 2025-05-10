using AuroraLib.Pixel.Image;
using System;

namespace AuroraLib.Pixel.PixelProcessor.Helper
{
    /// <summary>
    /// Provides access to a single row of pixels in an read only image, either as a buffer or direct reference.
    /// </summary>
    /// <typeparam name="TColor">The color type of the pixels.</typeparam>
    public readonly ref struct ReadOnlyRowAccessor<TColor> where TColor : unmanaged, IColor<TColor>
    {
        private readonly Span<TColor> Buffer;
        private readonly ReadOnlySpan<TColor> Pixel;
        private readonly IReadOnlyImage<TColor> Image;
        private readonly int Width;
        private readonly int X;
        private readonly int Stride;

        private readonly bool IsBuffered => Stride == -1;

        public ReadOnlyRowAccessor(IReadOnlyImage<TColor> image, int xOfset, int width)
        {
            X = xOfset;
            Width = width;
            Image = image;
            if (image is IReadOnlyImageSpan<TColor> imageSpan)
            {
                Stride = imageSpan.Stride;
                Pixel = imageSpan.Pixel;
                Buffer = default;
            }
            else
            {
                Stride = -1;
                Buffer = new TColor[width];
                Pixel = default;
            }
        }

        public ReadOnlySpan<TColor> this[int y]
        {
            get
            {
                if (IsBuffered)
                {
                    Image.GetPixel(X, y, Buffer);
                    return Buffer;
                }
                else
                {
                    return Pixel.Slice(y * Stride + X, Width);
                }
            }
        }
    }
}
