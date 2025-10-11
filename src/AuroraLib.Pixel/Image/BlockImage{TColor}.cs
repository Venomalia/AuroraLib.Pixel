using AuroraLib.Pixel.BlockProcessor;
using AuroraLib.Pixel.Metadata;
using AuroraLib.Pixel.Processing;
using AuroraLib.Pixel.Processing.Processor;
using System;
using System.Buffers;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace AuroraLib.Pixel.Image
{
    /// <summary>
    /// Represents a block-based image with direct row access and raw memory storage.
    /// Supports decoding/encoding via a block processor.
    /// </summary>
    /// <typeparam name="TColor">The color type of the image.</typeparam>
    public sealed class BlockImage<TColor> : IDirectRowAccess<TColor>, IBlockImage
        where TColor : unmanaged, IColor<TColor>
    {
        private readonly IBlockProcessor<TColor> _block;
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
        public ImageMetadata? Metadata { get; set; }
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

        public BlockImage(IBlockProcessor<TColor> processor, Memory<byte> memory, int width, int height)
        {
            if (memory.IsEmpty) throw new AggregateException(nameof(memory));
            int size = processor.CalculatedDataSize(width, height);
#if NET8_0_OR_GREATER
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
            ArgumentOutOfRangeException.ThrowIfLessThan(memory.Span.Length, size);
#else
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
            if (memory.Span.Length < size) throw new ArgumentOutOfRangeException(nameof(memory));
#endif
            _block = processor;
            _blockMemory = memory.Slice(0, size);
            Width = width;
            Height = height;
            Stride = _block.CalculateBlocksPerRow(width) * _block.BlockWidth;

            _buffer = ArrayPool<TColor>.Shared.Rent(Stride * _block.BlockHeight);
            _currentBlockLine = -1;
        }

        public BlockImage(IBlockProcessor<TColor> processor, IMemoryOwner<byte> memory, int width, int height) : this(processor, memory.Memory, width, height)
            => _disposable = memory;

        public BlockImage(IBlockProcessor<TColor> processor, int width, int height) : this(processor, MemoryPool<byte>.Shared.Rent(processor.CalculatedDataSize(width, height)), width, height)
            => _blockMemory.Span.Clear();

        private void DecodeBlockLine(int index)
        {
            if (_currentBlockLine == index)
                return;

            if (_isDirty)
                EncodeBlockLine();

            ReadOnlySpan<byte> source = _blockMemory.Span;
            Span<TColor> buffer = _buffer.AsSpan(0, Stride * _block.BlockHeight);
            int blockWidth = _block.BlockWidth;
            int bytesPerBlock = _block.BytesPerBlock;
            int blocksPerLine = Stride / blockWidth;
            int blockStart = index * blocksPerLine;

            for (int b = 0; b < blocksPerLine; b++)
            {
                ReadOnlySpan<byte> blockData = source.Slice((blockStart + b) * bytesPerBlock, bytesPerBlock);
                _block.DecodeBlock(blockData, buffer.Slice(b * blockWidth), Stride);
            }
            _currentBlockLine = index;
        }

        private void EncodeBlockLine()
        {
            if (_currentBlockLine == -1)
                return;

            Span<byte> source = _blockMemory.Span;
            ReadOnlySpan<TColor> buffer = _buffer.AsSpan(0, Stride * _block.BlockHeight);
            int blockWidth = _block.BlockWidth;
            int bytesPerBlock = _block.BytesPerBlock;
            int blocksPerLine = Stride / blockWidth;
            int blockStart = _currentBlockLine * blocksPerLine;

            for (int b = -0; b < blocksPerLine; b++)
            {
                Span<byte> blockData = source.Slice((blockStart + b) * bytesPerBlock, bytesPerBlock);
                _block.EncodeBlock(buffer.Slice(b * blockWidth), blockData, Stride);
            }
            _isDirty = false;
        }

        /// <inheritdoc/>
        public TColor this[int x, int y]
        {
            get
            {
                ThrowIfOutOfBounds(x, y);
                DecodeBlockLine(y / _block.BlockHeight);
                int localY = y % _block.BlockHeight * Stride;
                return _buffer[localY + x];
            }
            set
            {
                ThrowIfOutOfBounds(x, y);
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
            Span<byte> source = _blockMemory.Span;
            // If not compressed
            if (Unsafe.SizeOf<TColor>() * _block.BlockWidth * _block.BlockHeight == _block.BytesPerBlock)
            {
                source.Clear();
            }
            else
            {
                _buffer.AsSpan().Clear();
                _currentBlockLine = 0;
                EncodeBlockLine();

                int bytesPerBlockLine = _block.BytesPerBlock * (Stride / _block.BlockWidth);
                var defaultBock = source.Slice(0, bytesPerBlockLine);
                for (int i = bytesPerBlockLine; i < source.Length; i += bytesPerBlockLine)
                {
                    defaultBock.CopyTo(source.Slice(i, bytesPerBlockLine));
                }
            }
            _currentBlockLine = -1;

        }

        /// <inheritdoc/>
        public IImage<TColor> Clone()
        {
            if (_isDirty)
                EncodeBlockLine();

            BlockImage<TColor> clone = new BlockImage<TColor>(_block, Width, Height);
            Raw.CopyTo(clone.Raw);
            return clone;
        }

        IImage IReadOnlyImage.Clone() => Clone();

        IImage<TColor> IReadOnlyImage<TColor>.Create(int width, int height) => new BlockImage<TColor>(_block, width, height);

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
                _currentBlockLine = -1;
                return;
            }

            using MemoryImage<TColor> buffer = new MemoryImage<TColor>(region.Width, region.Height);
            Rectangle target = new Rectangle(0, 0, region.Width, region.Height);
            buffer.CopyFrom(this, region, target.Location);
            _currentBlockLine = -1;
            Height = region.Height;
            Width = region.Width;
            Stride = _block.CalculateBlocksPerRow(Width) * _block.BlockWidth;
            _blockMemory = _blockMemory.Slice(0, _block.CalculatedDataSize(region.Width, region.Height));
            _blockMemory.Span.Clear();
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

        private void ThrowIfOutOfBounds(int x, int y)
        {
            if ((uint)x >= Width)
                MemoryImage<TColor>.ThrowOutOfBoundsOfImage(x, Width, nameof(x), nameof(Width));
            if ((uint)y >= Height)
                MemoryImage<TColor>.ThrowOutOfBoundsOfImage(y, Height, nameof(y), nameof(Height));
        }
    }
}
