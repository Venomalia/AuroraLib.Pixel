using System;

namespace AuroraLib.Pixel.Processing.Resampler
{
    /// <summary>
    /// Resampling filter using a cubic convolution kernel.
    /// The bSpline and cardinal parameters control the shape of the cubic curve and allow
    /// different interpolation characteristics such as Catmull-Rom, Mitchell and B-Spline.
    /// </summary>
    public sealed class CubicResampler : IResampler
    {
        private readonly float bSpline;
        private readonly float cardinal;

        /// <inheritdoc/>
        public float Radius => 2;

        /// <summary>
        /// Initializes a new instance of the <see cref="CubicResampler"/> class.
        /// </summary>
        /// <param name="b">The B-spline parameter.</param>
        /// <param name="c">The Cardinal parameter.</param>
        public CubicResampler(float b = 0f, float c = 0.5f)
        {
            bSpline = b;
            cardinal = c;
        }
        /// <summary>
        /// A Catmull-Rom cubic filter.
        /// </summary>
        public static readonly CubicResampler CatmullRom = new CubicResampler(0f, 0.5f);

        /// <summary>
        /// A Mitchell-Netravali cubic filter.
        /// </summary>
        public static readonly CubicResampler Mitchell = new CubicResampler(1f / 3f, 1f / 3f);

        /// <summary>
        /// A cubic B-spline filter.
        /// </summary>
        public static readonly CubicResampler BSpline = new CubicResampler(1f, 0f);

        /// <summary>
        /// A Robidoux cubic filter.
        /// </summary>
        public static readonly CubicResampler Robidoux = new CubicResampler(.37821575509399867f, .31089212245300067f);

        /// <summary>
        /// A Robidoux Sharp cubic filter.
        /// </summary>
        public static readonly CubicResampler RobidouxSharp = new CubicResampler(.2620145123990142f, .3689927438004929f);

        /// <inheritdoc/>
        public float GetWeight(float x)
        {
            x = Math.Abs(x);
            float xx = x * x;

            if (x < 1f)
            {
                return (
                    ((12f - 9f * bSpline - 6f * cardinal) * x * xx) +
                    ((-18f + 12f * bSpline + 6f * cardinal) * xx) +
                    (6f - 2f * bSpline)) / 6f;
            }

            if (x < 2f)
            {
                return (
                    ((-bSpline - 6f * cardinal) * x * xx) +
                    ((6f * bSpline + 30f * cardinal) * xx) +
                    ((-12f * bSpline - 48f * cardinal) * x) +
                    (8f * bSpline + 24f * cardinal)) / 6f;
            }

            return 0f;
        }
    }
}
