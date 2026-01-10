using AuroraLib.Pixel.Image;
using AuroraLib.Pixel.PixelFormats;
using AuroraLib.Pixel.Processing;
using SkiaSharp;
using System;
using System.Runtime.CompilerServices;

namespace AuroraLib.Pixel.SkiaSharpExtension
{
    public static class Extensions
    {

        public static BGRA<byte> ToBGRA32(this SKColor color) => (BGRA<byte>)(uint)color;

        public static IImage AsAuroraImage(this SKBitmap bitmap)
            => bitmap.PeekPixels().AsAuroraImage();

        public static IImage AsAuroraImage(this SKImage image)
            => image.PeekPixels().AsAuroraImage();

        public static IImage AsAuroraImage(this SKPixmap pixmap) => pixmap.ColorType switch
        {
            SKColorType.Alpha8 => AsAuroraImage<A<byte>>(pixmap),
            SKColorType.Rgb565 => AsAuroraImage<RGB565>(pixmap),
            SKColorType.Argb4444 => AsAuroraImage<RGBA16>(pixmap),
            SKColorType.Rgba8888 => AsAuroraImage<RGBA<byte>>(pixmap),
            SKColorType.Rgb888x => AsAuroraImage<RGBA<byte>>(pixmap),
            SKColorType.Bgra8888 => AsAuroraImage<BGRA<byte>>(pixmap),
            SKColorType.Rgba1010102 => AsAuroraImage<RGBA1010102>(pixmap),
            SKColorType.Rgb101010x => AsAuroraImage<RGBA1010102>(pixmap),
            SKColorType.Gray8 => AsAuroraImage<I<byte>>(pixmap),
            SKColorType.RgbaF16 => AsAuroraImage<RGBA<Half>>(pixmap),
            SKColorType.RgbaF16Clamped => AsAuroraImage<RGBA<Half>>(pixmap),
            SKColorType.RgbaF32 => AsAuroraImage<RGBA<float>>(pixmap),
            SKColorType.Rg88 => AsAuroraImage<IA<byte>>(pixmap),
            SKColorType.AlphaF16 => AsAuroraImage<A<Half>>(pixmap),
            SKColorType.RgF16 => AsAuroraImage<IA<Half>>(pixmap),
            SKColorType.Alpha16 => AsAuroraImage<A<ushort>>(pixmap),
            SKColorType.Rg1616 => AsAuroraImage<IA<ushort>>(pixmap),
            SKColorType.Rgba16161616 => AsAuroraImage<RGBA<ushort>>(pixmap),
            SKColorType.Bgra1010102 => AsAuroraImage<BGRA1010102>(pixmap),
            SKColorType.Bgr101010x => AsAuroraImage<BGRA1010102>(pixmap),
            //SKColorType.Bgr101010xXR => throw new NotImplementedException(),
            SKColorType.Srgba8888 => AsAuroraImage<RGBA<byte>>(pixmap),
            SKColorType.R8Unorm => AsAuroraImage<I<byte>>(pixmap),
            //SKColorType.Rgba10x6 => throw new NotImplementedException(),
            _ => throw new NotSupportedException($"PixelFormat {pixmap.ColorType} is not supported."),
        };

        private static MemoryImage<TColor> AsAuroraImage<TColor>(SKPixmap pixmap) where TColor : unmanaged, IColor<TColor>
        {
            var data = new SKPixmapMemoryManager<TColor>(pixmap);
            return new MemoryImage<TColor>(data, pixmap.Width, pixmap.Height, pixmap.RowBytes / Unsafe.SizeOf<TColor>());
        }

        public static SKBitmap CloneAsSKBitmap(this IReadOnlyImage source, SKColorType colorType, SKAlphaType alphaType = SKAlphaType.Opaque)
        {
            SKBitmap clone = new SKBitmap(source.Width, source.Height, colorType, alphaType);
            using (IImage memoryImage = clone.AsAuroraImage())
                memoryImage.CopyFrom(source);

            return clone;
        }
    }
}
