using AuroraLib.Pixel.Image;

namespace AuroraLib.Pixel.Processing.Processor
{
    /// <summary>
    /// Represents an abstract base class for processors that operate on three images.
    /// </summary>
    public abstract class TripleImageProcessor : DoubleImageProcessor
    {
        /// <summary>
        /// The second source image from which data is read during processing.
        /// </summary>
        protected IReadOnlyImage SourceImageB { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TripleImageProcessor"/> class.
        /// </summary>
        /// <param name="sourceA">The first source image.</param>
        /// <param name="sourceB">The second source image.</param>
        protected TripleImageProcessor(IReadOnlyImage sourceA, IReadOnlyImage sourceB) : base(sourceA)
            => SourceImageB = sourceB;

        /// <inheritdoc/>
        protected override void Apply<TColorTarget, TColorA>(IImage<TColorTarget> target, IReadOnlyImage<TColorA> sourceA)
            => SourceImageB.Apply(new SecondPixelProcessorActivator<TColorTarget, TColorA>(target, sourceA, this));

        /// <summary>
        /// Applies the image operation using a target and two source images.
        /// </summary>
        protected abstract void Apply<TColorT, TColorA, TColorB>(IImage<TColorT> target, IReadOnlyImage<TColorA> sourceA, IReadOnlyImage<TColorB> sourceB)
            where TColorT : unmanaged, IColor<TColorT>
            where TColorA : unmanaged, IColor<TColorA>
            where TColorB : unmanaged, IColor<TColorB>;

        private sealed class SecondPixelProcessorActivator<TColorTarget, TColorA> : IReadOnlyPixelProcessor where TColorTarget : unmanaged, IColor<TColorTarget> where TColorA : unmanaged, IColor<TColorA>
        {
            private readonly IImage<TColorTarget> _target;
            private readonly IReadOnlyImage<TColorA> _imageA;
            private readonly TripleImageProcessor _context;

            public SecondPixelProcessorActivator(IImage<TColorTarget> target, IReadOnlyImage<TColorA> imageA, TripleImageProcessor context)
            {
                _target = target;
                _imageA = imageA;
                _context = context;
            }

            public void Apply<TColorB>(IReadOnlyImage<TColorB> imageB) where TColorB : unmanaged, IColor<TColorB>
                => _context.Apply(_target, _imageA, imageB);
        }
    }
}
