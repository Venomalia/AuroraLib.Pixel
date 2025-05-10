using System;
using System.Collections.Generic;

namespace AuroraLib.Pixel.Image
{
    /// <summary>
    /// Represents a writable indexed image with a modifiable color palette and access to pixel indices.
    /// </summary>
    /// <typeparam name="TColor">The actual color type stored in the palette.</typeparam>
    public interface IPaletteImage<TColor> : IReadOnlyPaletteImage<TColor>, IImage<TColor> where TColor : unmanaged, IColor<TColor>
    {
        /// <summary>
        /// Gets a writable List of <typeparamref name="TColor"/> representing the palette.
        /// </summary>
        new Span<TColor> Palette { get; }

        /// <summary>
        /// Sets the index of the color in the palette for the pixel at the specified position (x, y).
        /// </summary>
        /// <param name="x">The horizontal coordinate of the pixel.</param>
        /// <param name="y">The vertical coordinate of the pixel.</param>
        /// <param name="index">The index in the palette to be assigned to the pixel.</param>
        void SetPixelIndex(int x, int y, int index);

        /// <summary>
        /// Returns the internal pixel buffer.
        /// </summary>
        /// <returns>An untyped <see cref="IImage"/> representing the raw pxel buffer of the image.</returns>
        IImage GetBuffer();
    }
}
