using AuroraLib.Core.Buffers;
using AuroraLib.Pixel.Image;
using AuroraLib.Pixel.Metadata;
using AuroraLib.Pixel.PixelFormats;
using AuroraLib.Pixel.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Numerics;

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
                Type t when t == typeof(A8) => TryAsAuroraImage<A<byte>>(image, out memoryImage),
                Type t when t == typeof(Abgr32) => TryAsAuroraImage<ABGR<byte>>(image, out memoryImage),
                Type t when t == typeof(Argb32) => TryAsAuroraImage<ARGB<byte>>(image, out memoryImage),
                Type t when t == typeof(Bgr24) => TryAsAuroraImage<BGR<byte>>(image, out memoryImage),
                Type t when t == typeof(Bgr565) => TryAsAuroraImage<RGB565>(image, out memoryImage),
                Type t when t == typeof(Bgra32) => TryAsAuroraImage<BGRA<byte>>(image, out memoryImage),
                Type t when t == typeof(Bgra4444) => TryAsAuroraImage<ARGB16>(image, out memoryImage),
                Type t when t == typeof(Bgra5551) => TryAsAuroraImage<ARGB1555>(image, out memoryImage),
                Type t when t == typeof(Byte4) => TryAsAuroraImage<RGBA<byte>>(image, out memoryImage),
                Type t when t == typeof(L16) => TryAsAuroraImage<I<ushort>>(image, out memoryImage),
                Type t when t == typeof(L8) => TryAsAuroraImage<I<byte>>(image, out memoryImage),
                Type t when t == typeof(La16) => TryAsAuroraImage<IA<byte>>(image, out memoryImage),
                Type t when t == typeof(La32) => TryAsAuroraImage<IA<ushort>>(image, out memoryImage),
                Type t when t == typeof(Rgb24) => TryAsAuroraImage<RGB<byte>>(image, out memoryImage),
                Type t when t == typeof(Rgb48) => TryAsAuroraImage<RGB<ushort>>(image, out memoryImage),
                Type t when t == typeof(HalfVector4) => TryAsAuroraImage<RGBA<Half>>(image, out memoryImage),
                Type t when t == typeof(RgbaVector) => TryAsAuroraImage<RGBA<float>>(image, out memoryImage),
                Type t when t == typeof(Rgba1010102) => TryAsAuroraImage<RGBA1010102>(image, out memoryImage),
                Type t when t == typeof(Rgba32) => TryAsAuroraImage<RGBA<byte>>(image, out memoryImage),
                Type t when t == typeof(Rgba64) => TryAsAuroraImage<RGBA<ushort>>(image, out memoryImage),
                Type t when t == typeof(Rg32) => TryAsAuroraImage<IA<ushort>>(image, out memoryImage),
                _ => false,
            };

            static bool TryAsAuroraImage<TColor>(Image<TPixel> image, out Pixel.Image.IImage? memoryImage) where TColor : unmanaged, IColor<TColor>
            {
                if (image.DangerousTryGetSinglePixelMemory(out Memory<TPixel> memory))
                {
                    memoryImage = new MemoryImage<TColor>(memory.Cast<TPixel, TColor>(), image.Width, image.Height, image.Width);
                    memoryImage.Metadata = new ImageMetadata();

                    memoryImage.Metadata.Profiles.Add("SixLabors", image.Metadata);

                    Vector2 resolution = new Vector2((float)image.Metadata.HorizontalResolution, (float)image.Metadata.VerticalResolution);
                    memoryImage.Metadata.PixelsPerInch = image.Metadata.ResolutionUnits switch
                    {
                        SixLabors.ImageSharp.Metadata.PixelResolutionUnit.PixelsPerCentimeter => resolution * 2.54f,
                        SixLabors.ImageSharp.Metadata.PixelResolutionUnit.PixelsPerMeter => resolution * 0.0254f,
                        _ => resolution,
                    };
                    if (image.Metadata.IccProfile != null)
                        memoryImage.Metadata.Icc = image.Metadata.IccProfile.ToByteArray();

                    if (image.Metadata.ExifProfile != null)
                        memoryImage.Metadata.Exif = image.Metadata.ExifProfile.ToByteArray();

                    if (image.Metadata.XmpProfile != null)
                        memoryImage.Metadata.Xmp = image.Metadata.XmpProfile.ToByteArray();
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

            if (source.Metadata != null)
            {
                clone.Metadata.ResolutionUnits = SixLabors.ImageSharp.Metadata.PixelResolutionUnit.PixelsPerInch;
                clone.Metadata.HorizontalResolution = source.Metadata.PixelsPerInch.X;
                clone.Metadata.VerticalResolution = source.Metadata.PixelsPerInch.Y;

                if (source.Metadata.Icc != null)
                    clone.Metadata.IccProfile = new SixLabors.ImageSharp.Metadata.Profiles.Icc.IccProfile(source.Metadata.Icc);

                if (source.Metadata.Exif != null)
                    clone.Metadata.ExifProfile = new SixLabors.ImageSharp.Metadata.Profiles.Exif.ExifProfile(source.Metadata.Exif);

                if (source.Metadata.Xmp != null)
                    clone.Metadata.XmpProfile = new SixLabors.ImageSharp.Metadata.Profiles.Xmp.XmpProfile(source.Metadata.Xmp);
            }
            return clone;
        }
    }
}
