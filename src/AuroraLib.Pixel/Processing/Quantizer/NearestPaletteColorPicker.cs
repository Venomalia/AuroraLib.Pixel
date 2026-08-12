using AuroraLib.Pixel.Image;
using System;
using System.Numerics;

namespace AuroraLib.Pixel.Processing.Quantizer
{
    /// <summary>
    /// A palette quantizer that selects the most similar existing palette color without modifying the palette.
    /// </summary>
    /// <typeparam name="TColor">The type representing a color.</typeparam>
    public sealed class NearestPaletteColorPicker<TColor> : IColorQuantizer<TColor>
        where TColor : unmanaged, IColor<TColor>
    {
        /// <inheritdoc/>
        public int ResolveColor(IPaletteImage<TColor> image, TColor newColor, int newColorCount = 1)
        {
            ReadOnlySpan<TColor> palette = image.Palette.Span;
            Vector4 color = newColor.ToScaledVector4();

            int bestIndex = 0;
            float minDistance = float.MaxValue;

            for (int i = 0; i < palette.Length; i++)
            {
                float distance = CalculateColorDistance(color, palette[i].ToScaledVector4());

                if (distance < minDistance)
                {
                    minDistance = distance;
                    bestIndex = i;
                }
            }

            return bestIndex;
        }

        private static float CalculateColorDistance(Vector4 color1, Vector4 color2)
        {
            Vector4 difference = Vector4.Abs(color1 - color2);
            return difference.Length() * (1f + difference.W);
        }
    }
}
