using AuroraLib.Pixel.Image;
using AuroraLib.Pixel.PixelProcessor.Helper;
using AuroraLib.Pixel.Processing.Processor;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using System;

namespace AuroraLib.Pixel.ImageSharpExtension
{
    internal class CopyToImageSharpProcessor<TPixel> : IReadOnlyPixelProcessor where TPixel : unmanaged, IPixel<TPixel>
    {
        private readonly Image<TPixel> Target;

        public CopyToImageSharpProcessor(Image<TPixel> target) => Target = target;

        public void Apply<TColor>(IReadOnlyImage<TColor> image) where TColor : unmanaged, IColor<TColor>
        {
            CopyFrom(Target, image);
        }

        private static void CopyFrom<TColor>(Image<TPixel> target, IReadOnlyImage<TColor> source)
            where TColor : unmanaged, IColor<TColor>
        {
            ReadOnlyRowAccessor<TColor> sourcePixel = new ReadOnlyRowAccessor<TColor>(source, 0, source.Width);
            for (int y = 0; y < source.Height; y++)
            {
                Span<TPixel> targetRow = target.DangerousGetPixelRowMemory(y).Span;
                ReadOnlySpan<TColor> sourceRow = sourcePixel[y];

                for (int x = 0; x < sourceRow.Length; x++)
                {
                    targetRow[x].FromScaledVector4(sourceRow[x].ToScaledVector4());
                }
            }
        }
    }
}
