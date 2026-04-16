using System;
using System.Numerics;

namespace AuroraLib.Pixel.Metadata
{
    /// <summary>
    /// Defines an RGB color space using CIE XY chromaticity coordinates.
    /// </summary>
    public sealed class ColorSpace : IEquatable<ColorSpace>
    {
        /// <summary>
        /// The white point of the color space in CIE XY coordinates.
        /// </summary>
        public Vector2 WhitePoint { get; }

        /// <summary>
        /// The red primary of the color space in CIE XY coordinates.
        /// </summary>
        public Vector2 Red { get; }

        /// <summary>
        /// The green primary of the color space in CIE XY coordinates.
        /// </summary>
        public Vector2 Green { get; }

        /// <summary>
        /// The blue primary of the color space in CIE XY coordinates.
        /// </summary>
        public Vector2 Blue { get; }

        public ColorSpace(Vector2 whitePoint, Vector2 red, Vector2 green, Vector2 blue)
        {
            WhitePoint = whitePoint;
            Red = red;
            Green = green;
            Blue = blue;
        }

        /// <summary>
        /// Standard sRGB color space using D65 white point and IEC 61966-2-1 primaries.
        /// </summary>
        public static readonly ColorSpace sRGB = new ColorSpace(new Vector2(0.3127f, 0.3290f), new Vector2(0.64f, 0.33f), new Vector2(0.30f, 0.60f), new Vector2(0.15f, 0.06f));

        /// <summary>
        /// Display P3 color space using D65 white point.
        /// </summary>
        public static readonly ColorSpace DisplayP3 = new ColorSpace(new Vector2(0.3127f, 0.3290f), new Vector2(0.68f, 0.32f), new Vector2(0.265f, 0.69f), new Vector2(0.15f, 0.06f));

        /// <summary>
        /// Adobe RGB (1998) color space using D65 white point.
        /// </summary>
        public static readonly ColorSpace AdobeRGB = new ColorSpace(new Vector2(0.3127f, 0.3290f), new Vector2(0.64f, 0.33f), new Vector2(0.21f, 0.71f), new Vector2(0.15f, 0.06f));

        /// <inheritdoc/>
        public bool Equals(ColorSpace? other) => other != null && other.WhitePoint == WhitePoint && other.Red == Red && other.Green == Green && other.Blue == Blue;
    }

}
