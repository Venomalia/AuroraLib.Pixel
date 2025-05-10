using AuroraLib.Pixel.Image;
using AuroraLib.Pixel.PixelFormats;
using AuroraLib.Pixel.Processing;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;

namespace AuroraLib.Pixel.BitmapExtension
{
    public static class Extensions
    {
        /// <summary>
        /// Converts a <see cref="Bitmap"/> to an Aurora <see cref="IImage"/> format based on the bitmap's pixel format, accessing the bitmap's memory directly without copying.
        /// </summary>
        /// <param name="bitmap">The Bitmap to convert to an Aurora Image.</param>
        /// <returns>An Aurora Image representation of the bitmap, directly accessing its memory.</returns>
        public static IImage AsAuroraImage(this Bitmap bitmap) => bitmap.PixelFormat switch
        {
            //PixelFormat.Format1bppIndexed => throw new NotImplementedException(),
            //PixelFormat.Format4bppIndexed => throw new NotImplementedException(),
            PixelFormat.Format8bppIndexed => AsAuroraPaletteImage<I8>(bitmap),
            PixelFormat.Format16bppGrayScale => AsAuroraImage<I16>(bitmap),
            PixelFormat.Format16bppRgb555 => AsAuroraImage<RGB555>(bitmap),
            PixelFormat.Format16bppRgb565 => AsAuroraImage<RGB565>(bitmap),
            PixelFormat.Format16bppArgb1555 => AsAuroraImage<ARGB1555>(bitmap),
            PixelFormat.Format24bppRgb => AsAuroraImage<BGR24>(bitmap),
            PixelFormat.Format32bppRgb => AsAuroraImage<BGRA32>(bitmap),
            PixelFormat.Format32bppArgb => AsAuroraImage<BGRA32>(bitmap),
            PixelFormat.Format32bppPArgb => AsAuroraImage<BGRA32>(bitmap),
            //PixelFormat.Format48bppRgb => AsAuroraImage<BGR48>(bitmap), // it's not 48 bpp, but rather 39 bpp with a gamma of 1!
            //PixelFormat.Format64bppArgb => AsAuroraImage<BGRA64>(bitmap), // Same here.
            //PixelFormat.Format64bppPArgb => AsAuroraImage<BGRA64>(bitmap), // Same here.
            _ => throw new NotSupportedException($"PixelFormat {bitmap.PixelFormat} is not supported."),
        };

        /// <summary>
        /// Converts an <see cref="IReadOnlyImage"/> to a <see cref="Bitmap"/> with the specified pixel <paramref name="format"/>.
        /// </summary>
        /// <param name="source">The IReadOnlyImage to convert to a Bitmap.</param>
        /// <param name="format">The pixel format to use for the resulting Bitmap. Defaults to <see cref="PixelFormat.Format32bppArgb"/>.</param>
        /// <returns>A Bitmap representation of the IReadOnlyImage in the specified format.</returns>
        public static Bitmap CloneAsBitmap(this IReadOnlyImage source, PixelFormat format = PixelFormat.Format32bppArgb)
        {
            Bitmap clone = new Bitmap(source.Width, source.Height, format);
            using (IImage image = clone.AsAuroraImage())
            {
                image.CopyFrom(source);
            }
            return clone;
        }

        private static MemoryImage<TColor> AsAuroraImage<TColor>(Bitmap bitmap) where TColor : unmanaged, IColor<TColor>
        {
            var data = new BitmapMemoryManager<TColor>(bitmap);
            return new MemoryImage<TColor>(data, data.Data.Width, data.Data.Height, data.Data.Stride / Unsafe.SizeOf<TColor>());
        }

        private static MemoryPaletteImage<TColor, BGRA32> AsAuroraPaletteImage<TColor>(Bitmap bitmap) where TColor : unmanaged, IColor<TColor>, IIndexColor
        {
            var data = new BitmapMemoryManager<TColor>(bitmap);
            var imageData = new MemoryImage<TColor>(data, data.Data.Width, data.Data.Height, data.Data.Stride / Unsafe.SizeOf<TColor>());
            var bitmapPalette = bitmap.Palette.Entries.AsSpan();

            var paletteImage = new MemoryPaletteImage<TColor, BGRA32>(imageData, bitmapPalette.Length);
            var auroraPalette = paletteImage.Palette;
            for (int i = 0; i < bitmapPalette.Length; i++)
                auroraPalette[i] = bitmapPalette[i];

            data.DisposeAction = () => UpdateBitmapPalette(bitmap, paletteImage);
            return paletteImage;
        }

        private static void UpdateBitmapPalette(Bitmap bitmap, IPaletteImage<BGRA32> paletteImage)
        {
            var bitmapPalette = bitmap.Palette.Entries.AsSpan();
            var auroraPalette = paletteImage.Palette;
            for (int i = 0; i < bitmapPalette.Length; i++)
                bitmapPalette[i] = auroraPalette[i];
        }
    }
}
