using AuroraLib.Pixel.BlockProcessor;
using System;

namespace AuroraLib.Pixel.Image
{
    /// <summary>
    /// Represents a image as a block of raw memory with a defined format.
    /// </summary>
    public interface IBlockImage : IImage
    {
        /// <summary>
        /// Gets the block format used to encode/decode the texture data.
        /// </summary>
        IBlockFormat BlockFormat { get; }

        /// <summary>
        /// Gets the number of pixels per row (stride).
        /// </summary>
        int Stride { get; }

        /// <summary>
        /// Gets a span representing the raw byte data of the texture.
        /// </summary>
        Span<byte> Raw { get; }
    }
}
