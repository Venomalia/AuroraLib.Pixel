using AuroraLib.Pixel.Image;
using System;

namespace AuroraLib.Pixel.Processing.Quantizer
{
    /// <summary>
    /// Defines a quantizer that maps a new color to a palette index in a palette-based image, creating space in the palette if necessary so that the new color can be added.
    /// </summary>
    /// <typeparam name="TColor">The type representing a color.</typeparam>
    public interface IColorQuantizer<TColor>
        where TColor : unmanaged, IColor<TColor>
    {
        /// <summary>
        /// Resolves the specified color to an index in the palette, the quantizer may adjust the palette to create space for it.
        /// </summary>
        /// <param name="image">The palette-based image to modify if necessary.</param>
        /// <param name="newColor">The color to resolve and insert into the palette.</param>
        /// <param name="newColorCount">The number of pixels of this color being added.</param>
        /// <returns>The palette index assigned to the resolved color.</returns>
        int ResolveColor(IPaletteImage<TColor> image, TColor newColor, int newColorCount = 1);
    }
}
