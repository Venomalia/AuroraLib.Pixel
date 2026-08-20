using AuroraLib.Pixel.Image;
using System;
using System.Drawing;
using System.Numerics;

namespace AuroraLib.Pixel.Processing.Processor
{
    /// <summary>
    /// Inverts the color channels of an image.
    /// </summary>
    public sealed class InvertProcessor : IPixelProcessor
    {
        /// <inheritdoc/>
        public void Apply<TColor>(IImage<TColor> image, Rectangle region) where TColor : unmanaged, IColor<TColor>
        {
            if (default(TColor).FormatInfo.HasColor)
                image.Apply(Invert, region);
        }

        private static void Invert<TColor>(Span<TColor> pixels) where TColor : unmanaged, IColor<TColor>
        {
            for (int i = 0; i < pixels.Length; i++)
            {
                Vector4 color = pixels[i].ToScaledVector4();
                color.X = 1f - color.X;
                color.Y = 1f - color.Y;
                color.Z = 1f - color.Z;
                pixels[i].FromScaledVector4(color);
            }
        }
    }
}
