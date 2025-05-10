using System;

namespace AuroraLib.Pixel.BlockProcessor
{
    /// <summary>
    /// Provides functionality to encode and decode rectangular blocks of image or texture data using a specified pixel format.
    /// </summary>
    /// <typeparam name="TColor"> The pixel format used for processing.</typeparam>
    public interface IBlockProcessor<TColor> : IBlockFormat where TColor : unmanaged, IColor<TColor>
    {
        /// <summary>
        /// Decodes a block of compressed or formatted data into a set of pixels.
        /// </summary>
        /// <param name="source">The input data span containing the encoded or compressed block.</param>
        /// <param name="target">The destination span where the decoded pixels will be written.</param>
        /// <param name="stride">The number of pixels per row in the output pixel buffer (used to calculate row offsets).</param>
        void DecodeBlock(ReadOnlySpan<byte> source, Span<TColor> target, int stride);

        /// <summary>
        /// Encodes a block of pixels into a compressed or formatted block of data.
        /// </summary>
        /// <param name="source">The source span containing raw pixel data to be encoded.</param>
        /// <param name="target">The output span where the encoded block will be written.</param>
        /// <param name="stride">The number of pixels per row in the input pixel buffer (used to calculate row offsets).</param>
        void EncodeBlock(ReadOnlySpan<TColor> source, Span<byte> target, int stride);
    }
}
