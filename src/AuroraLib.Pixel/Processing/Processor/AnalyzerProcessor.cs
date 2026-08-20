using AuroraLib.Pixel.Image;
using AuroraLib.Pixel.Processing.Analyzer;
using System.Drawing;

namespace AuroraLib.Pixel.Processing.Processor
{
    internal sealed class AnalyzerProcessor<TResult> : IReadOnlyPixelProcessor
    {
        public Analyzer<TResult> Analyzer { get; }
        public TResult Result { get; private set; }

        public AnalyzerProcessor(Analyzer<TResult> analyzer)
            => Analyzer = analyzer;

        public void Apply<TColor>(IReadOnlyImage<TColor> image, Rectangle region) where TColor : unmanaged, IColor<TColor>
            => Result = Analyzer.Analyze(image, region);
    }
}
