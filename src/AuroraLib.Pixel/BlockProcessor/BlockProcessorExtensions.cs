using AuroraLib.Pixel.Image;
using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace AuroraLib.Pixel.BlockProcessor
{
    public static class BlockProcessorExtensions
    {
        /// <summary>
        /// Calculates the number of blocks per row based on the <paramref name="width"/>.
        /// </summary>
        /// <param name="format">The block format.</param>
        /// <param name="width">The width to divide.</param>
        /// <returns>The number of blocks per row.</returns>
        public static int CalculateBlocksPerRow(this IBlockFormat format, int width)
            => (width + format.BlockWidth - 1) / format.BlockWidth;

        /// <summary>
        /// Calculates the number of blocks per column based on the <paramref name="height"/>.
        /// </summary>
        /// <param name="format">The block format.</param>
        /// <param name="height">The height to divide.</param>
        /// <returns>The number of blocks per column.</returns>
        public static int CalculateBlocksPerColumn(this IBlockFormat format, int height)
            => (height + format.BlockHeight - 1) / format.BlockHeight;

        /// <summary>
        /// Calculates the total number of blocks based on the <paramref name="width"/> and <paramref name="height"/>.
        /// </summary>
        /// <param name="format">The block format.</param>
        /// <param name="width">The width to divide.</param>
        /// <param name="height">The height to divide.</param>
        /// <returns>The total number of blocks.</returns>
        public static int CalculateBlockCount(this IBlockFormat format, int width, int height)
            => CalculateBlocksPerRow(format, width) * CalculateBlocksPerColumn(format, height);

        /// <summary>
        /// Calculates the total data size for the given area based on the <paramref name="width"/> and <paramref name="height"/>.
        /// </summary>
        /// <param name="format">The block format.</param>
        /// <param name="width">The width of the area.</param>
        /// <param name="height">The height of the area.</param>
        /// <returns>The total data size in bytes.</returns>
        public static int CalculatedDataSize(this IBlockFormat format, int width, int height)
            => CalculateBlockCount(format, width, height) * format.BytesPerBlock;

        /// <summary>
        /// Decodes an image from a byte <paramref name="source"/> into an image object.
        /// </summary>
        /// <typeparam name="TColor">The color type of the image pixels.</typeparam>
        /// <param name="processor">The processor used to decode the image blocks.</param>
        /// <param name="source">The byte array containing the image data.</param>
        /// <param name="width">The width of the image to decode.</param>
        /// <param name="height">The height of the image to decode.</param>
        /// <returns>A decoded image with the specified dimensions.</returns>
        public static IImage<TColor> DecodeImage<TColor>(this IBlockProcessor<TColor> processor, ReadOnlySpan<byte> source, int width, int height) where TColor : unmanaged, IColor<TColor>
        {
            int blocksPerLine = processor.CalculateBlocksPerRow(width);
            int blocksPerRow = processor.CalculateBlocksPerColumn(height);
            int stride = blocksPerLine * processor.BlockWidth;

            int expected = blocksPerLine * blocksPerRow * processor.BytesPerBlock;
            if (source.Length < expected)
                ThrowSourceBufferLength(source.Length, expected);

            IMemoryOwner<TColor> pixel = MemoryPool<TColor>.Shared.Rent(stride * blocksPerRow * processor.BlockHeight);
            MemoryImage<TColor> image = new MemoryImage<TColor>(pixel, width, height, stride);
            DecodeImageBuffer(processor, source, image.Pixel, stride);
            return image;
        }

        /// <summary>
        /// Decodes image data from a byte <paramref name="source"/> into a <paramref name="destination"/> pixel buffer.
        /// </summary>
        /// <typeparam name="TColor">The color type of the image pixels.</typeparam>
        /// <param name="processor">The processor used to decode the image blocks.</param>
        /// <param name="source">The byte array containing the image data.</param>
        /// <param name="destination">The destination pixel buffer to store the decoded image.</param>
        /// <param name="stride">The stride (row width in pixels) of the destination image buffer.</param>
        public static void DecodeImageBuffer<TColor>(this IBlockProcessor<TColor> processor, ReadOnlySpan<byte> source, Span<TColor> destination, int stride) where TColor : unmanaged, IColor<TColor>
        {
            int bytesPerBlock = processor.BytesPerBlock;
            int blockWidth = processor.BlockWidth;
            int blockHeight = processor.BlockHeight;

            int blocksPerLine = stride / blockWidth;
            int pixelPerBlockLine = stride * blockHeight;
            int bytesPerBlockLine = blocksPerLine * bytesPerBlock;
            int blocksPerRow = source.Length / bytesPerBlockLine;

            // Validation: Ensure the target buffer is large enough to hold the decoded pixels
            int expectedTargetLength = blocksPerLine * blocksPerRow * blockWidth * blockHeight;
            if (destination.Length < expectedTargetLength)
                throw new ArgumentException($"Target buffer is too small. Expected: {expectedTargetLength} pixels, but got: {destination.Length} pixels.", nameof(destination));

            // Decode each block row
            for (int yBlock = 0; yBlock < blocksPerRow; yBlock++)
            {
                ReadOnlySpan<byte> blockLine = source.Slice(yBlock * bytesPerBlockLine, bytesPerBlockLine);
                Span<TColor> pixelLine = destination.Slice(yBlock * pixelPerBlockLine, pixelPerBlockLine);

                // Decode each block within the row
                for (int xBlock = 0; xBlock < blocksPerLine; xBlock++)
                {
                    ReadOnlySpan<byte> sourceBlock = blockLine.Slice(xBlock * bytesPerBlock, bytesPerBlock);
                    Span<TColor> targetBlockPixels = pixelLine.Slice(xBlock * blockWidth);
                    processor.DecodeBlock(sourceBlock, targetBlockPixels, stride);
                }
            }
        }

        /// <summary>
        /// Decodes image data from a byte <paramref name="source"/> into the specified <paramref name="destination"/> image.
        /// </summary>
        /// <typeparam name="TColor">The color type of the image pixels.</typeparam>
        /// <param name="processor">The processor used to decode the image blocks.</param>
        /// <param name="source">The byte array containing the image data.</param>
        /// <param name="destination">The image where the decoded data will be stored.</param>
        public static void DecodeImage<TColor>(this IBlockProcessor<TColor> processor, ReadOnlySpan<byte> source, IImage<TColor> destination) where TColor : unmanaged, IColor<TColor>
        {
            int bytesPerBlock = processor.BytesPerBlock;
            int blockWidth = processor.BlockWidth;
            int blockHeight = processor.BlockHeight;

            int blocksPerLine = processor.CalculateBlocksPerRow(destination.Width);
            int blocksPerRow = processor.CalculateBlocksPerColumn(destination.Height);
            int stride = blocksPerLine * blockWidth;

            int expected = blocksPerLine * blocksPerRow * bytesPerBlock;
            if (source.Length < expected)
                ThrowSourceBufferLength(source.Length, expected);

            // Fast path: directly decode into contiguous pixel buffer if layout matches
            if (destination is IImageSpan<TColor> targetISpan && targetISpan.Stride == stride && targetISpan.Pixel.Length >= stride * blocksPerRow * blockHeight)
            {
                DecodeImageBuffer(processor, source, targetISpan.Pixel, stride);
                return;
            }

            int bytesPerBlockLine = blocksPerLine * bytesPerBlock;
            int pixelPerBlockLine = stride * blockHeight;

            Span<TColor> buffer = new TColor[pixelPerBlockLine];
            // Decode each block row
            for (int yBlock = 0; yBlock < blocksPerRow; yBlock++)
            {
                ReadOnlySpan<byte> blockLine = source.Slice(yBlock * bytesPerBlockLine, bytesPerBlockLine);

                // Decode each block within the row
                for (int xBlock = 0; xBlock < blocksPerLine; xBlock++)
                {
                    ReadOnlySpan<byte> sourceBlock = blockLine.Slice(xBlock * bytesPerBlock, bytesPerBlock);
                    Span<TColor> targetBlockPixels = buffer.Slice(xBlock * blockWidth);
                    processor.DecodeBlock(sourceBlock, targetBlockPixels, stride);
                }

                int startY = yBlock * blockHeight;
                int linesToCopy = yBlock == blocksPerRow - 1 ? destination.Height - startY : blockHeight;

                for (int row = 0; row < linesToCopy; row++)
                {
                    destination.SetPixel(0, startY + row, buffer.Slice(row * stride, destination.Width));
                }
            }
        }

        /// <summary>
        /// Encodes an image into a byte array.
        /// </summary>
        /// <typeparam name="TColor">The color type of the image pixels.</typeparam>
        /// <param name="processor">The processor used to encode the image blocks.</param>
        /// <param name="source">The source image to encode.</param>
        /// <returns>A memory owner containing the encoded image bytes.</returns>
        public static IMemoryOwner<byte> EncodeImage<TColor>(this IBlockProcessor<TColor> processor, IReadOnlyImage<TColor> source) where TColor : unmanaged, IColor<TColor>
        {
            int bufferSize = processor.CalculatedDataSize(source.Width, source.Height);
            IMemoryOwner<byte> memory = MemoryPool<byte>.Shared.Rent(bufferSize);
            processor.EncodeImage(memory.Memory.Span.Slice(0, bufferSize), source);
            return memory;
        }

        /// <summary>
        /// Encodes pixel data from the <paramref name="source"/> image into the <paramref name="destination"/> buffer.
        /// </summary>
        /// <typeparam name="TColor">The color type of the image pixels.</typeparam>
        /// <param name="processor">The processor used to encode the image blocks.</param>
        /// <param name="source">The source pixel data to encode.</param>
        /// <param name="destination">The destination buffer to store the encoded image bytes.</param>
        /// <param name="stride">The stride (row width in pixels) of the source image buffer.</param>
        public static void EncodeImageBuffer<TColor>(this IBlockProcessor<TColor> processor, ReadOnlySpan<TColor> source, Span<byte> destination, int stride) where TColor : unmanaged, IColor<TColor>
        {
            int bytesPerBlock = processor.BytesPerBlock;
            int blockWidth = processor.BlockWidth;
            int blockHeight = processor.BlockHeight;

            int blocksPerLine = stride / blockWidth;
            int pixelPerBlockLine = stride * blockHeight;
            int bytesPerBlockLine = blocksPerLine * bytesPerBlock;
            int blocksPerRow = source.Length / pixelPerBlockLine;

            // Validation: Ensure the destination buffer is large enough to hold the encode pixels
            int expectedDestinationLength = blocksPerLine * blocksPerRow * bytesPerBlock;
            if (destination.Length < expectedDestinationLength)
                throw new ArgumentException($"Destination buffer is too small. Expected: {expectedDestinationLength} bytes, but got: {destination.Length} bytes.", nameof(destination));

            // Encode each block row
            for (int yBlock = 0; yBlock < blocksPerRow; yBlock++)
            {
                Span<byte> blockLine = destination.Slice(yBlock * bytesPerBlockLine, bytesPerBlockLine);
                ReadOnlySpan<TColor> pixelLine = source.Slice(yBlock * pixelPerBlockLine, pixelPerBlockLine);

                // Encode each block within the row
                for (int xBlock = 0; xBlock < blocksPerLine; xBlock++)
                {
                    Span<byte> destinationBlock = blockLine.Slice(xBlock * bytesPerBlock, bytesPerBlock);
                    ReadOnlySpan<TColor> sourceBlockPixels = pixelLine.Slice(xBlock * blockWidth, stride);
                    processor.EncodeBlock(sourceBlockPixels, destinationBlock, stride);
                }
            }
        }

        /// <summary>
        /// Encodes an image from the <paramref name="source"/> to the <paramref name="destination"/> buffer.
        /// </summary>
        /// <typeparam name="TColor">The color type of the image pixels.</typeparam>
        /// <param name="processor">The processor used to encode the image blocks.</param>
        /// <param name="destination">The destination buffer to store the encoded image bytes.</param>
        /// <param name="source">The source image to encode.</param>
        public static void EncodeImage<TColor>(this IBlockProcessor<TColor> processor, Span<byte> destination, IReadOnlyImage<TColor> source) where TColor : unmanaged, IColor<TColor>
        {
            int bytesPerBlock = processor.BytesPerBlock;
            int blockWidth = processor.BlockWidth;
            int blockHeight = processor.BlockHeight;

            int blocksPerLine = processor.CalculateBlocksPerRow(source.Width);
            int blocksPerRow = processor.CalculateBlocksPerColumn(source.Height);
            int stride = blocksPerLine * blockWidth;

            int expected = blocksPerLine * blocksPerRow * bytesPerBlock;
            if (destination.Length < expected)
                throw new ArgumentException($"Destination buffer is too small. Expected at least {expected} bytes, but got {destination.Length}.", nameof(destination));

            // Fast path: directly encode from contiguous pixel buffer if layout matches
            if (source is IReadOnlyImageSpan<TColor> sourceISpan && sourceISpan.Stride == stride && sourceISpan.Pixel.Length >= stride * blocksPerRow * blockHeight)
            {
                processor.EncodeImageBuffer(sourceISpan.Pixel, destination, stride);
                return;
            }

            int bytesPerBlockLine = blocksPerLine * bytesPerBlock;
            int pixelPerBlockLine = stride * blockHeight;

            Span<TColor> buffer = new TColor[pixelPerBlockLine];

            // Decode each block row
            for (int yBlock = 0; yBlock < blocksPerRow; yBlock++)
            {
                // Clears pixels outside the boundary.
                buffer.Clear();

                int startY = yBlock * blockHeight;
                int linesToCopy = yBlock == blocksPerRow - 1 ? source.Height - startY : blockHeight;

                for (int row = 0; row < linesToCopy; row++)
                {
                    source.GetPixel(0, startY + row, buffer.Slice(row * stride, source.Width));
                }

                Span<byte> blockLine = destination.Slice(yBlock * bytesPerBlockLine, bytesPerBlockLine);

                // Decode each block within the row
                for (int xBlock = 0; xBlock < blocksPerLine; xBlock++)
                {
                    Span<byte> destinationBlock = blockLine.Slice(xBlock * bytesPerBlock, bytesPerBlock);
                    ReadOnlySpan<TColor> sourceBlockPixels = buffer.Slice(xBlock * blockWidth, stride);
                    processor.EncodeBlock(sourceBlockPixels, destinationBlock, stride);
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowSourceBufferLength(int sourceLength, int expected)
            => throw new ArgumentException($"Source buffer is too small. Expected at least {expected} bytes, but got {sourceLength}.");
    }
}
