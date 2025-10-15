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

        public static BGRA32 ToBGRA32(this SKColor color) => (BGRA32)(uint)color;

        public static IImage AsAuroraImage(this SKBitmap bitmap)
            => bitmap.PeekPixels().AsAuroraImage();

        public static IImage AsAuroraImage(this SKImage image)
            => image.PeekPixels().AsAuroraImage();

        public static IImage AsAuroraImage(this SKPixmap pixmap) => pixmap.ColorType switch
        {
            SKColorType.Alpha8 => AsAuroraImage<A8>(pixmap),
            SKColorType.Rgb565 => AsAuroraImage<RGB565>(pixmap),
            SKColorType.Argb4444 => AsAuroraImage<RGBA16>(pixmap),
            SKColorType.Rgba8888 => AsAuroraImage<RGBA32>(pixmap),
            SKColorType.Rgb888x => AsAuroraImage<RGBA32>(pixmap),
            SKColorType.Bgra8888 => AsAuroraImage<BGRA32>(pixmap),
            SKColorType.Rgba1010102 => AsAuroraImage<RGBA1010102>(pixmap),
            SKColorType.Rgb101010x => AsAuroraImage<RGBA1010102>(pixmap),
            SKColorType.Gray8 => AsAuroraImage<I8>(pixmap),
            SKColorType.RgbaF16 => AsAuroraImage<RGBAf64>(pixmap),
            SKColorType.RgbaF16Clamped => AsAuroraImage<RGBAf64>(pixmap),
            SKColorType.RgbaF32 => AsAuroraImage<RGBAf128>(pixmap),
            SKColorType.Rg88 => AsAuroraImage<IA16>(pixmap),
            SKColorType.AlphaF16 => AsAuroraImage<Af16>(pixmap),
            SKColorType.RgF16 => AsAuroraImage<IA32>(pixmap),
            SKColorType.Alpha16 => AsAuroraImage<A16>(pixmap),
            SKColorType.Rg1616 => AsAuroraImage<IA32>(pixmap),
            SKColorType.Rgba16161616 => AsAuroraImage<RGBA64>(pixmap),
            SKColorType.Bgra1010102 => AsAuroraImage<BGRA1010102>(pixmap),
            SKColorType.Bgr101010x => AsAuroraImage<BGRA1010102>(pixmap),
            //SKColorType.Bgr101010xXR => throw new NotImplementedException(),
            SKColorType.Srgba8888 => AsAuroraImage<RGBA32>(pixmap),
            //SKColorType.R8Unorm => throw new NotImplementedException(),
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
