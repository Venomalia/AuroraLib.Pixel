using System;
using System.Collections.Generic;
using System.Text;

namespace AuroraLib.Pixel.Processing.Resampler
{
    public static class Resamplers
    {
        /// <inheritdoc cref="NearestNeighborResampler"/>
        public static readonly NearestNeighborResampler NearestNeighbor = new NearestNeighborResampler();

        /// <inheritdoc cref="BoxResampler"/>
        public static readonly BoxResampler Box = new BoxResampler();

        /// <inheritdoc cref="LinearResampler"/>
        public static readonly LinearResampler Bilinear = new LinearResampler();

        /// <inheritdoc cref="CubicResampler.CatmullRom"/>
        public static readonly CubicResampler CatmullRom = CubicResampler.CatmullRom;

        /// <inheritdoc cref="CubicResampler.Mitchell"/>
        public static readonly CubicResampler Mitchell = CubicResampler.Mitchell;

        /// <inheritdoc cref="CubicResampler.BSpline"/>
        public static readonly CubicResampler BSpline = CubicResampler.BSpline;

        /// <inheritdoc cref="CubicResampler.Robidoux"/>
        public static readonly CubicResampler Robidoux = CubicResampler.Robidoux;

        /// <inheritdoc cref="CubicResampler.RobidouxSharp"/>
        public static readonly CubicResampler RobidouxSharp = CubicResampler.RobidouxSharp;

        /// <inheritdoc cref="CubicResampler"/>
        public static readonly CubicResampler Bicubic = CatmullRom;

        /// <inheritdoc cref="LanczosResampler"/>
        public static readonly LanczosResampler Lanczos2 = new LanczosResampler(2);

        /// <inheritdoc cref="LanczosResampler"/>
        public static readonly LanczosResampler Lanczos3 = new LanczosResampler(3);

        /// <inheritdoc cref="LanczosResampler"/>
        public static readonly LanczosResampler Lanczos4 = new LanczosResampler(4);

    }
}
