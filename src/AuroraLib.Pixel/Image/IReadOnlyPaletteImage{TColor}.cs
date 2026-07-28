using System;

namespace AuroraLib.Pixel.Image
{
    /// <summary>
    /// Represents a read-only indexed image with a color palette.
    /// </summary>
    /// <typeparam name="TColor">The actual color type stored in the palette.</typeparam>
    public interface IReadOnlyPaletteImage<TColor> : IReadOnlyImage<TColor>, IReadOnlyPaletteImage where TColor : unmanaged, IColor<TColor>
    {
        /// <summary>
        /// Gets a read-only list of <typeparamref name="TColor"/> representing the palette.
        /// </summary>
        ReadOnlySpan<TColor> Palette { get; }
    }
}
