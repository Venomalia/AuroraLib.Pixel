using System;

namespace AuroraLib.Pixel.Image
{
    /// <summary>
    /// Represents a read-only indexed image with a color palette.
    /// </summary>
    /// <typeparam name="TColor">The actual color type stored in the palette.</typeparam>
    public interface IReadOnlyPaletteImage<TColor> : IReadOnlyImage<TColor> where TColor : unmanaged, IColor<TColor>
    {
        /// <summary>
        /// Gets the number of colors currently used in the palette.
        /// </summary>
        int ColorsUsed { get; }

        /// <summary>
        /// Gets a read-only list of <typeparamref name="TColor"/> representing the palette.
        /// </summary>
        ReadOnlySpan<TColor> Palette { get; }

        /// <summary>
        /// Retrieves the index of the color in the palette for the pixel at the specified position (x, y).
        /// </summary>
        /// <param name="x">The horizontal coordinate of the pixel.</param>
        /// <param name="y">The vertical coordinate of the pixel.</param>
        /// <returns>The index in the palette corresponding to the pixel at the specified coordinates.</returns>
        int GetPixelIndex(int x, int y);
    }
}
