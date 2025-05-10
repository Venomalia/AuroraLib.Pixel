using System.Numerics;

namespace AuroraLib.Pixel.PixelProcessor
{
    /// <summary>
    /// Provides common blend modes used for compositing two colors.
    /// </summary>
    public static class BlendModes
    {
        /// <summary>
        /// A delegate that defines a blending function taking a target and source color.
        /// </summary>
        /// <param name="baseColor">The destination color vector (e.g., the background).</param>
        /// <param name="blendColor">The source color vector to be blended over the target.</param>
        /// <param name="intensity">The intensity of the blending.</param>
        /// <returns>The resulting blended color.</returns>
        public delegate Vector4 BlendFunction(Vector4 baseColor, Vector4 blendColor, float intensity);

        /// <summary>
        /// Normal Blend Mode (Standard Alpha Blending).
        /// </summary>
        public static Vector4 Normal(Vector4 baseColor, Vector4 blendColor, float intensity)
            => AlphaBlend(baseColor, blendColor, blendColor.W * intensity);

        /// <summary>
        /// Multiply Blend Mode.
        /// </summary>
        public static Vector4 Multiply(Vector4 baseColor, Vector4 blendColor, float intensity)
            => AlphaBlend(baseColor, baseColor * blendColor, blendColor.W * intensity);

        /// <summary>
        /// Divide Blend Mode.
        /// </summary>
        public static Vector4 Divide(Vector4 baseColor, Vector4 blendColor, float intensity)
            => AlphaBlend(baseColor, Vector4.Min(baseColor / blendColor, Vector4.One), blendColor.W * intensity);

        /// <summary>
        /// Addition Blend Mode.
        /// </summary>
        public static Vector4 Add(Vector4 baseColor, Vector4 blendColor, float intensity)
            => AlphaBlend(baseColor, Vector4.Min(baseColor + blendColor, Vector4.One), blendColor.W * intensity);

        /// <summary>
        /// Subtract Blend Mode.
        /// </summary>
        public static Vector4 Subtract(Vector4 baseColor, Vector4 blendColor, float intensity)
            => AlphaBlend(baseColor, Vector4.Max(baseColor - blendColor, Vector4.Zero), blendColor.W * intensity);

        /// <summary>
        /// Difference Blend Mode.
        /// </summary>
        public static Vector4 Difference(Vector4 baseColor, Vector4 blendColor, float intensity)
            => AlphaBlend(baseColor, Vector4.Abs(baseColor - blendColor), blendColor.W * intensity);

        /// <summary>
        /// Screen Blend Mode.
        /// </summary>
        public static Vector4 Screen(Vector4 baseColor, Vector4 blendColor, float intensity)
        {
            Vector4 blended = Vector4.One - (Vector4.One - baseColor) * (Vector4.One - blendColor);
            return AlphaBlend(baseColor, blended, blendColor.W * intensity);
        }

        /// <summary>
        /// Darken Blend Mode.
        /// </summary>
        public static Vector4 Darken(Vector4 baseColor, Vector4 blendColor, float intensity)
        {
            Vector4 blended = Vector4.Min(baseColor, blendColor);
            return AlphaBlend(baseColor, blended, blendColor.W * intensity);
        }

        /// <summary>
        /// Lighten Blend Mode.
        /// </summary>
        public static Vector4 Lighten(Vector4 baseColor, Vector4 blendColor, float intensity)
        {
            Vector4 blended = Vector4.Max(baseColor, blendColor);
            return AlphaBlend(baseColor, blended, blendColor.W * intensity);
        }

        /// <summary>
        /// Overlay Blend Mode.
        /// </summary>
        public static Vector4 Overlay(Vector4 target, Vector4 source, float intensity)
            => AlphaBlend(target, OverlayBlend(target, source), source.W * intensity);

        /// <summary>
        /// HardLight Blend Mode.
        /// </summary>
        public static Vector4 HardLight(Vector4 target, Vector4 source, float intensity)
            => AlphaBlend(target, OverlayBlend(source, target), source.W * intensity);

        /// <summary>
        /// SoftLight Blend Mode.
        /// </summary>
        public static Vector4 SoftLight(Vector4 baseColor, Vector4 blendColor, float intensity)
        {
            Vector4 multi = baseColor * blendColor;
            Vector4 blendedColor = (Vector4.One - 2f * blendColor) * multi + 2f * multi;
            return AlphaBlend(baseColor, blendedColor, blendColor.W * intensity);
        }

        private static Vector4 AlphaBlend(Vector4 baseColor, Vector4 blendColor, float intensity)
        {
            Vector4 blendedColor = Vector4.Lerp(baseColor, blendColor, intensity);
            blendedColor.W = MixAlpha(baseColor.W, intensity);
            return blendedColor;
        }

        private static float MixAlpha(float target, float source)
            => 1 - (1 - source) * (1 - target);

        private static Vector4 OverlayBlend(Vector4 baseColor, Vector4 blendColor)
            => new Vector4(OverlayChannel(baseColor.X, blendColor.X),
                           OverlayChannel(baseColor.Y, blendColor.Y),
                           OverlayChannel(baseColor.Z, blendColor.Z),
                           1.0f);

        private static float OverlayChannel(float target, float source)
            => target < 0.5f ?
            2f * target * source :
            1f - 2f * (1f - target) * (1f - source);
    }
}
