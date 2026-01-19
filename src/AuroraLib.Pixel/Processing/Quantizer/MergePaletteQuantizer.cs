using AuroraLib.Pixel.Image;
using System;
using System.Buffers;
using System.Numerics;

namespace AuroraLib.Pixel.Processing.Quantizer
{
    /// <summary>
    /// A palette quantizer that merges the most similar colors in the palette.
    /// </summary>
    /// <typeparam name="TColor">The type representing a color.</typeparam>
    public sealed class MergePaletteQuantizer<TColor> : IColorQuantizer<TColor>
        where TColor : unmanaged, IColor<TColor>
    {
        /// <inheritdoc/>
        public int ResolveColor(IPaletteImage<TColor> image, TColor newColor, int newColorCount = 1)
        {
            var palette = image.Palette;
            // Convert all palette colors to Vector4 for distance calculations, Extra slot at the end for the new color.
            using IMemoryOwner<Vector4> memoryBuffer = MemoryPool<Vector4>.Shared.Rent(palette.Length + 1);
            Span<Vector4> buffer = memoryBuffer.Memory.Span.Slice(0, palette.Length + 1);
            for (int i = 0; i < palette.Length; i++)
            {
                buffer[i] = palette[i].ToScaledVector4();
            }
            buffer[palette.Length] = newColor.ToScaledVector4();

            // Find the two most similar colors in the palette.
            (int color1, int color2) = FindMostSimilarColors(buffer);

            int usage1 = image.PaletteRefCounts[color1];
            int usage2 = color2 == palette.Length ? newColorCount : image.PaletteRefCounts[color2];

            // Fail save: If the first color slot is unused, place the new color there directly
            if (usage1 == 0)
            {
                palette[color1] = newColor;
                return color1;
            }
            float weight = (float)usage2 / (usage1 + usage2);

            // Merge the two colors into color1, leaning towards the more frequently used one.
            palette[color1].FromScaledVector4(Vector4.Lerp(buffer[color1], buffer[color2], weight));

            // If the second color isn't the new one, replace it in the image
            if (color2 != palette.Length)
            {
                image.ReplaceColor(color2, color1);
                palette[color2] = newColor;
                return color2;
            }
            return color1;
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
        }

        private static float CalculateColorDistance(Vector4 color1, Vector4 color2)
        {
            Vector4 Difference = Vector4.Abs(color1 - color2);
            return Difference.Length() * (1f + Difference.W);
        }

    }
}
