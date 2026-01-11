using AuroraLib.Pixel.PixelFormats;
using System;
using System.Buffers.Binary;

namespace AuroraLib.Pixel.BlockProcessor
{
    /// <summary>
    /// Y210 (YUV 4:2:2, 10-bit) block processor.
    /// </summary>
    public sealed class Y210 : IBlockProcessor<Y410>
    {
        /// <inheritdoc/>
        public int BlockWidth => 2;
        /// <inheritdoc/>
        public int BlockHeight => 1;
        /// <inheritdoc/>
        public int BytesPerBlock => 8;

        /// <inheritdoc/>
        public void DecodeBlock(ReadOnlySpan<byte> source, Span<Y410> target, int _)
        {
            ushort u = BinaryPrimitives.ReadUInt16LittleEndian(source);
            ushort y0 = BinaryPrimitives.ReadUInt16LittleEndian(source.Slice(2, 2));
            ushort v = BinaryPrimitives.ReadUInt16LittleEndian(source.Slice(4, 2));
            ushort y1 = BinaryPrimitives.ReadUInt16LittleEndian(source.Slice(6, 2));

            target[0] = Y410.PackNativUYVA(u, y0, v, 0b11);
            target[1] = Y410.PackNativUYVA(u, y1, v, 0b11);
        }

        /// <inheritdoc/>
        public void EncodeBlock(ReadOnlySpan<Y410> source, Span<byte> target, int _)
        {
            // Take chroma from the first pixel
            ushort u = (ushort)(source[0].U >> 6);
            ushort v = (ushort)(source[0].V >> 6);
            ushort y0 = (ushort)(source[0].Y >> 6);
            ushort y1 = (ushort)(source[1].Y >> 6);

            BinaryPrimitives.WriteUInt16LittleEndian(target, u);
            BinaryPrimitives.WriteUInt16LittleEndian(target.Slice(2, 2), y0);
            BinaryPrimitives.WriteUInt16LittleEndian(target.Slice(4, 2), v);
            BinaryPrimitives.WriteUInt16LittleEndian(target.Slice(6, 2), y1);
        }
    }
}
