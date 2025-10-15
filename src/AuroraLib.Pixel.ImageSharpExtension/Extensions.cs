using AuroraLib.Core.Buffers;
using AuroraLib.Pixel.Image;
using AuroraLib.Pixel.Metadata;
using AuroraLib.Pixel.PixelFormats;
using AuroraLib.Pixel.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using System;

namespace AuroraLib.Pixel.ImageSharpExtension
{
    public static class Extensions
    {
        /// <summary>
        /// Convert a <see cref="SixLabors.ImageSharp.Image"/> into an <see cref="IImage"/> representation.
        /// The conversion succeeds only if the pixel format is supported and the image buffer is stored in a contiguous block of memory.
        /// </summary>
        /// <param name="image">The source image to convert.</param>
        /// <returns>The <see cref="IImage"/> when successful.</returns>
        public static Pixel.Image.IImage AsAuroraImage(this SixLabors.ImageSharp.Image image)
        {
            if (TryAsAuroraImage(image, out Pixel.Image.IImage memoryImage))
            {
                return memoryImage;
            }
            throw new NotSupportedException();
        }

        /// <summary>
        /// Tries to convert a <see cref="SixLabors.ImageSharp.Image"/> into an <see cref="IImage"/> representation.
        /// The conversion succeeds only if the pixel format is supported and the image buffer is stored in a contiguous block of memory.
        /// </summary>
        /// <param name="image">The source image to convert.</param>
        /// <param name="memoryImage">When this method returns, contains the converted Aurora image if the conversion was successful; otherwise, <c>null</c>.</param>
        /// <returns><c>true</c> if the image was successfully converted; otherwise, <c>false</c>.</returns>
        public static bool TryAsAuroraImage(this SixLabors.ImageSharp.Image image, out Pixel.Image.IImage memoryImage)
        {
            var visitor = new TryAsAuroraHelper();
            image.AcceptVisitor(visitor);
            memoryImage = visitor.auroraImage;
            return memoryImage != null;
        }

        private class TryAsAuroraHelper : IImageVisitor
        {
            public Pixel.Image.IImage? auroraImage;

            public void Visit<TPixel>(Image<TPixel> image) where TPixel : unmanaged, IPixel<TPixel>
                => image.TryAsAuroraImage(out auroraImage);
        }

        /// <inheritdoc cref="TryAsAuroraImage(SixLabors.ImageSharp.Image, out Pixel.Image.IImage)"/>
        public static bool TryAsAuroraImage<TPixel>(this Image<TPixel> image, out Pixel.Image.IImage memoryImage) where TPixel : unmanaged, IPixel<TPixel>
        {
            memoryImage = null;
            return typeof(TPixel) switch
            {
                Type t when t == typeof(SixLabors.ImageSharp.PixelFormats.A8) => TryAsAuroraImage<PixelFormats.A8>(image, out memoryImage),
                Type t when t == typeof(Abgr32) => TryAsAuroraImage<ABGR32>(image, out memoryImage),
                Type t when t == typeof(Argb32) => TryAsAuroraImage<ARGB32>(image, out memoryImage),
                Type t when t == typeof(Bgr24) => TryAsAuroraImage<BGR24>(image, out memoryImage),
                Type t when t == typeof(Bgr565) => TryAsAuroraImage<RGB565>(image, out memoryImage),
                Type t when t == typeof(Bgra32) => TryAsAuroraImage<BGRA32>(image, out memoryImage),
                Type t when t == typeof(Bgra4444) => TryAsAuroraImage<ARGB16>(image, out memoryImage),
                Type t when t == typeof(Bgra5551) => TryAsAuroraImage<ARGB1555>(image, out memoryImage),
                Type t when t == typeof(Byte4) => TryAsAuroraImage<RGBA32>(image, out memoryImage),
                Type t when t == typeof(L16) => TryAsAuroraImage<I16>(image, out memoryImage),
                Type t when t == typeof(L8) => TryAsAuroraImage<I8>(image, out memoryImage),
                Type t when t == typeof(La16) => TryAsAuroraImage<IA16>(image, out memoryImage),
                Type t when t == typeof(La32) => TryAsAuroraImage<IA32>(image, out memoryImage),
                Type t when t == typeof(Rgb24) => TryAsAuroraImage<RGB24>(image, out memoryImage),
                Type t when t == typeof(Rgb48) => TryAsAuroraImage<RGB48>(image, out memoryImage),
                Type t when t == typeof(HalfVector4) => TryAsAuroraImage<RGBAf64>(image, out memoryImage),
                Type t when t == typeof(RgbaVector) => TryAsAuroraImage<RGBAf128>(image, out memoryImage),
                Type t when t == typeof(Rgba1010102) => TryAsAuroraImage<RGB48>(image, out memoryImage),
                Type t when t == typeof(Rgba32) => TryAsAuroraImage<RGBA32>(image, out memoryImage),
                Type t when t == typeof(Rgba64) => TryAsAuroraImage<RGBA64>(image, out memoryImage),
                _ => false,
            };

            static bool TryAsAuroraImage<TColor>(Image<TPixel> image, out Pixel.Image.IImage? memoryImage) where TColor : unmanaged, IColor<TColor>
            {
                if (image.DangerousTryGetSinglePixelMemory(out Memory<TPixel> memory))
                {
                    memoryImage = new MemoryImage<TColor>(memory.Cast<TPixel, TColor>(), image.Width, image.Height, image.Width);
                    memoryImage.Metadata = new ImageMetadata();
                    memoryImage.Metadata.Profiles.Add("SixLabors", image.Metadata);
                    if (image.Metadata.XmpProfile != null)
                        memoryImage.Metadata.XmpProfile = image.Metadata.XmpProfile.GetDocument();
                    return true;
                }
                memoryImage = null;
                return false;
            }
        }

        /// <summary>
        /// Creates a clone of the given <see cref="IReadOnlyImage"/> and returns it as a new <see cref="SixLabors.ImageSharp.Image{TPixel}"/>.
        /// </summary>
        /// <typeparam name="TPixel">The pixel type of the target image.</typeparam>
        /// <param name="source">The source image to clone.</param>
        /// <returns>A new <see cref="SixLabors.ImageSharp.Image{TPixel}"/> that is a clone of the source image.</returns>
        public static Image<TPixel> CloneAsImageSharp<TPixel>(this IReadOnlyImage source) where TPixel : unmanaged, IPixel<TPixel>
        {
            Image<TPixel> clone = new Image<TPixel>(source.Width, source.Height);

            if (clone.TryAsAuroraImage(out Pixel.Image.IImage memoryImage))
                memoryImage.CopyFrom(source);
            else
                source.Apply(new CopyToImageSharpProcessor<TPixel>(clone)); // use Vector4

            return clone;
        }
    }
}
