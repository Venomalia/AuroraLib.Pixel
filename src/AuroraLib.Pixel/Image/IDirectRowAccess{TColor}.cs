using System;

namespace AuroraLib.Pixel.Image
{
    /// <summary>
    /// Represents a mutable image that provides direct read and write access to its pixel data by row.
    /// Modifications to the returned span will be written back if necessary.
    /// </summary>
    /// <typeparam name="TColor">The pixel type of the image.</typeparam>
    public interface IDirectRowAccess<TColor> : IImage<TColor>, IReadOnlyDirectRowAccess<TColor>
        where TColor : unmanaged, IColor<TColor>
    {
        /// <summary>
        /// Gets a writable span representing the pixels of a specific row.
        /// Modifications will be written back if necessary.
        /// </summary>
        /// <param name="y">The 0-based row index.</param>
        /// <returns>A writable span containing the pixels of the specified row.</returns>
        Span<TColor> GetWritableRow(int y);
    }

}
