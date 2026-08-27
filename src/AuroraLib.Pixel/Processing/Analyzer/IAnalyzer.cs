using AuroraLib.Pixel.Image;
using System.Drawing;

namespace AuroraLib.Pixel.Processing.Analyzer
{
    public interface IAnalyzer<TResult>
    {
        TResult Analyze<TColor>(IReadOnlyImage<TColor> image, Rectangle region) where TColor : unmanaged, IColor<TColor>;
    }
}