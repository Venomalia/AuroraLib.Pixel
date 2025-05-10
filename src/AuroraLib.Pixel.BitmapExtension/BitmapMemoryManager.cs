using System;
using System.Buffers;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;

namespace AuroraLib.Pixel.BitmapExtension
{
    internal sealed class BitmapMemoryManager<T> : MemoryManager<T> where T : struct
    {
        internal readonly BitmapData Data;
        private readonly Bitmap Bitmap;
        private readonly int Pixel;
        internal Action? DisposeAction;

        public BitmapMemoryManager(Bitmap bitmap)
        {
            Bitmap = bitmap;
            Data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);

            int elementSize = Unsafe.SizeOf<T>();
            if (Data.Stride % elementSize != 0)
            {
                throw new InvalidOperationException($"Stride ({Data.Stride}) is not divisible by the element size ({elementSize}).");
            }

            Pixel = Data.Stride / elementSize * Data.Height;
        }

        public unsafe override Span<T> GetSpan()
            => new Span<T>(Data.Scan0.ToPointer(), Pixel);

        public unsafe override MemoryHandle Pin(int elementIndex = 0)
        {
            var pointer = (byte*)Data.Scan0.ToPointer() + elementIndex * Unsafe.SizeOf<T>();
            return new MemoryHandle(pointer);
        }

        public override void Unpin()
        { }

        protected override void Dispose(bool disposing)
        {
            DisposeAction?.Invoke();
            Bitmap.UnlockBits(Data);
        }
    }
}
