using SkiaSharp;
using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace AuroraLib.Pixel.SkiaSharpExtension
{
    internal class SKPixmapMemoryManager<TColor> : MemoryManager<TColor> where TColor : unmanaged, IColor<TColor>
    {
        private readonly SKPixmap _pixmap;
        private readonly int length;
        private unsafe TColor* _ptr;

        public unsafe SKPixmapMemoryManager(SKPixmap pixmap)
        {
            int elementSize = Unsafe.SizeOf<TColor>();
            if (elementSize != pixmap.BytesPerPixel)
                throw new InvalidOperationException();

            if (pixmap.RowBytes % elementSize != 0)
            {
                throw new InvalidOperationException($"Stride ({pixmap.RowBytes}) is not divisible by the element size ({elementSize}).");
            }

            _pixmap = pixmap ?? throw new ArgumentNullException(nameof(pixmap));
            _ptr = (TColor*)pixmap.GetPixels().ToPointer();
            length = pixmap.Height * (pixmap.RowBytes / Unsafe.SizeOf<TColor>());
        }

        public unsafe override Span<TColor> GetSpan()
            => new Span<TColor>(_ptr, length);

        public unsafe override MemoryHandle Pin(int elementIndex = 0)
        {
            if (elementIndex < 0 || elementIndex >= length)
                throw new ArgumentOutOfRangeException(nameof(elementIndex));

            return new MemoryHandle(_ptr + elementIndex);
        }

        public override void Unpin()
        { }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _pixmap.Dispose();
        }
    }
}