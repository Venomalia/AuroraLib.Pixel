using System;

namespace AuroraLib.Pixel.Image
{
    /// <summary>
    /// Represents a read-only image that provides direct access to its pixel data by row.
    /// The implementation may return a direct memory slice or a cached/decoded buffer.
    /// </summary>
    /// <typeparam name="TColor">The pixel type of the image.</typeparam>
    public interface IReadOnlyDirectRowAccess<TColor> : IReadOnlyImage<TColor>
        where TColor : unmanaged, IColor<TColor>
    {
        /// <summary>
        /// Gets a read-only span representing the pixels of a specific row.
        /// The implementation may return a direct slice of memory or a cached/decoded buffer.
        /// </summary>
        /// <param name="y">The 0-based row index.</param>
        /// <returns>A read-only span containing the pixels of the specified row.</returns>
        ReadOnlySpan<TColor> GetRow(int y);
    }

}
