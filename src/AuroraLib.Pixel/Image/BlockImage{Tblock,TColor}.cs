using AuroraLib.Pixel.BlockProcessor;
using AuroraLib.Pixel.Processing;
using AuroraLib.Pixel.Processing.Processor;
using System;
using System.Buffers;
using System.Drawing;
using System.Numerics;

namespace AuroraLib.Pixel.Image
{
    /// <summary>
    /// Represents a block-based image with direct row access and raw memory storage.
    /// Supports decoding/encoding via a block processor.
    /// </summary>
    /// <typeparam name="Tblock">The block processor type used to decode/encode blocks.</typeparam>
    /// <typeparam name="TColor">The color type of the image.</typeparam>
    public sealed class BlockImage<Tblock, TColor> : IDirectRowAccess<TColor>, IBlockImage
        where Tblock : IBlockProcessor<TColor>, new()
        where TColor : unmanaged, IColor<TColor>
    {
        private static readonly IBlockProcessor<TColor> _block = new Tblock();
        private Memory<byte> _blockMemory;
        private readonly IDisposable? _disposable;

        private readonly TColor[] _buffer;
        private int _currentBlockLine;
        private bool _isDirty;

        /// <inheritdoc/>
        public int Width { get; private set; }
        /// <inheritdoc/>
        public int Height { get; private set; }
        /// <inheritdoc/>
        public int Stride { get; private set; }
        /// <inheritdoc/>
        public IBlockFormat BlockFormat => _block;
        /// <inheritdoc/>
        public Span<byte> Raw
        {
            get
            {
                if (_isDirty)
                    EncodeBlockLine();
                return _blockMemory.Span;
            }
        }

        public BlockImage(Memory<byte> memory, int width, int height)
        {
            if (memory.IsEmpty) throw new AggregateException(nameof(memory));
            int size = _block.CalculatedDataSize(width, height);
#if NET8_0_OR_GREATER
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
            ArgumentOutOfRangeException.ThrowIfLessThan(memory.Span.Length, size);
#else
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
            if (memory.Span.Length < size) throw new ArgumentOutOfRangeException(nameof(memory));
#endif
            _blockMemory = memory.Slice(0, size);
            Width = width;
            Height = height;
            Stride = _block.CalculateBlocksPerRow(width) * _block.BlockWidth;

            _buffer = ArrayPool<TColor>.Shared.Rent(Stride * _block.BlockHeight);
            _currentBlockLine = -1;
        }

        public BlockImage(IMemoryOwner<byte> memory, int width, int height) : this(memory.Memory, width, height)
            => _disposable = memory;

        public BlockImage(int width, int height) : this(MemoryPool<byte>.Shared.Rent(_block.CalculatedDataSize(width, height)), width, height)
        { }

        private void DecodeBlockLine(int index)
        {
            if (_currentBlockLine == index)
                return;

            if (_isDirty)
                EncodeBlockLine();

            Span<TColor> buffer = _buffer.AsSpan(0, Stride * _block.BlockHeight);
            int blockWidth = _block.BlockWidth;
            int bytesPerBlock = _block.BytesPerBlock;
            int blocksPerLine = Stride / blockWidth;
            int blockStart = index * blocksPerLine;

            for (int b = 0; b < blocksPerLine; b++)
            {
                Span<byte> blockData = Raw.Slice((blockStart + b) * bytesPerBlock, bytesPerBlock);
                _block.DecodeBlock(blockData, buffer.Slice(b * blockWidth), Stride);
            }
            _currentBlockLine = index;
        }

        private void EncodeBlockLine()
        {
            if (_currentBlockLine == -1)
                return;

            Span<TColor> buffer = _buffer.AsSpan(0, Stride * _block.BlockHeight);
            int blockWidth = _block.BlockWidth;
            int bytesPerBlock = _block.BytesPerBlock;
            int blocksPerLine = Stride / blockWidth;
            int blockStart = _currentBlockLine * blocksPerLine;

            for (int b = 0; b < blocksPerLine; b++)
            {
                Span<byte> blockData = Raw.Slice((blockStart + b) * bytesPerBlock, bytesPerBlock);
                _block.EncodeBlock(buffer.Slice(b * blockWidth), blockData, Stride);
            }
            _isDirty = false;
        }

        /// <inheritdoc/>
        public TColor this[int x, int y]
        {
            get
            {
                DecodeBlockLine(y / _block.BlockHeight);
                int localY = y % _block.BlockHeight * Stride;
                return _buffer[localY + x];
            }
            set
            {
                DecodeBlockLine(y / _block.BlockHeight);
                int localY = y % _block.BlockHeight * Stride;
                _buffer[localY + x] = value;
                _isDirty = true;
            }
        }

        /// <inheritdoc/>
        public void Apply(IPixelProcessor processor) => processor.Apply(this);

        /// <inheritdoc/>
        public void Apply(IReadOnlyPixelProcessor processor) => processor.Apply(this);

        /// <inheritdoc/>
        public void Clear()
        {
            _isDirty = false;
            _currentBlockLine = -1;
            Raw.Clear();
        }

        /// <inheritdoc/>
        public IImage<TColor> Clone()
        {
            if (_isDirty)
                EncodeBlockLine();

            BlockImage<Tblock, TColor> clone = new BlockImage<Tblock, TColor>(Width, Height);
            Raw.CopyTo(clone.Raw);
            return clone;
        }

        IImage IReadOnlyImage.Clone() => Clone();

        /// <inheritdoc/>
        public Span<TColor> GetWritableRow(int y)
        {
            DecodeBlockLine(y / _block.BlockHeight);
            int localY = y % _block.BlockHeight * Stride;
            _isDirty = true;
            return _buffer.AsSpan(localY, Width);
        }

        /// <inheritdoc/>
        public ReadOnlySpan<TColor> GetRow(int y)
        {
            DecodeBlockLine(y / _block.BlockHeight);
            int localY = y % _block.BlockHeight * Stride;
            return _buffer.AsSpan(localY, Width);
        }

        /// <inheritdoc/>
        public void Crop(Rectangle region)
        {
            if (_isDirty)
                EncodeBlockLine();

            if (region.X < 0 || region.Y < 0 || region.X + region.Width > Width || region.Y + region.Height > Height)
                throw new ArgumentOutOfRangeException(nameof(region), "Crop region is outside the image bounds.");

            if (region.Width == Width && region.X == 0 && region.X % _block.BlockWidth == 0)
            {
                int blockStride = Stride / _block.BlockWidth;
                int start = region.Y / _block.BlockHeight * blockStride;
                _blockMemory = _blockMemory.Slice(start * _block.BytesPerBlock, region.Height * blockStride * _block.BytesPerBlock);
                Height = region.Height;
                return;
            }

            using MemoryImage<TColor> buffer = new MemoryImage<TColor>(region.Width, region.Height);
            Rectangle target = new Rectangle(0, 0, region.Width, region.Height);
            buffer.CopyFrom(this, target, region.Location);
            Height = region.Height;
            Width = region.Width;
            Stride = _block.CalculateBlocksPerRow(Width) * _block.BlockWidth;
            this.CopyFrom(buffer, target, target.Location);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_blockMemory.IsEmpty)
            {
                _disposable?.Dispose();
                _blockMemory = Memory<byte>.Empty;
                Width = Height = 0;
                _currentBlockLine = -1;
                ArrayPool<TColor>.Shared.Return(_buffer);
            }
        }

        void IReadOnlyImage<TColor>.GetPixel(int x, int y, Span<TColor> pixelRow)
        {
            if (pixelRow.IsEmpty)
                return;

            DecodeBlockLine(y / _block.BlockHeight);
            int localY = y % _block.BlockHeight * Stride;
            _buffer.AsSpan(localY + x, pixelRow.Length).CopyTo(pixelRow);
        }

        void IImage<TColor>.SetPixel(int x, int y, ReadOnlySpan<TColor> pixelRow)
        {
            if (pixelRow.IsEmpty)
                return;

            DecodeBlockLine(y / _block.BlockHeight);
            int localY = y % _block.BlockHeight * Stride;
            pixelRow.CopyTo(_buffer.AsSpan(localY + x, pixelRow.Length));
            _isDirty = true;
        }

        Vector4 IImage.this[int x, int y]
        {
            get => this[x, y].ToScaledVector4();
            set
            {
                TColor color = default;
                color.FromScaledVector4(value);
                this[x, y] = color;
            }
        }

        Vector4 IReadOnlyImage.this[int x, int y] => this[x, y].ToScaledVector4();
    }
}
