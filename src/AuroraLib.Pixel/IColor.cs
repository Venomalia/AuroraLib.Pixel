using AuroraLib.Pixel.Processing;
using System;
using System.Numerics;

namespace AuroraLib.Pixel
{

    /// <summary>
    /// Defines a strongly-typed interface for color structs, enabling conversion, comparison, and integration with generic color processing mechanisms.
    /// </summary>
    /// <typeparam name="TSelf">The implementing color type. Must be an unmanaged struct that implements <see cref="IColor{TSelf}"/>.</typeparam>
    public interface IColor<TSelf> : IColor, IEquatable<TSelf> where TSelf : unmanaged, IColor<TSelf>
    {
    }

    /// <summary>
    /// Interface for converting between color representations.
    /// </summary>
    public interface IColor
    {
        /// <summary>
        /// Gets or sets the normalized mask value of the color component.
        /// The value is in the range of [0, 1].
        /// </summary>
        float Mask { get; set; }

        /// <summary>
        /// Gets the pixel format metadata for this color.
        /// </summary>
        PixelFormatInfo FormatInfo { get; }

        /// <summary>
        /// Converts the color to a scaled <see cref="Vector4"/> representation.
        /// Each component (X = R, Y = G, Z = B, W = A) is in the range [0, 1].
        /// </summary>
        /// <returns>A <see cref="Vector4"/> representing the scaled color.</returns>
        Vector4 ToScaledVector4();

        /// <summary>
        /// Converts a <see cref="Vector4"/> back to the color.
        /// Each component (X = R, Y = G, Z = B, W = A) is in the range [0, 1].
        /// </summary>
        /// <param name="vector">A <see cref="Vector4"/> containing the scaled color values.</param>
        void FromScaledVector4(Vector4 vector);

        /// <summary>
        /// Converts the current color to an RGBA representation and writes it into the specified target.
        /// </summary>
        /// <typeparam name="TColor">The target color type implementing <see cref="IRGBA{T}"/>.</typeparam>
        /// <param name="value">A reference to the destination value where the RGBA components will be written.</param>
        void ToRGBA<TColor>(ref TColor value) where TColor : IRGBA<byte>;

        /// <summary>
        /// Populates the current color by converting from another 8-bit RGB or RGBA color type.
        /// </summary>
        /// <typeparam name="TColor">The source color type, which must implement <see cref="IRGB{T}"/> with 8-bit channels.</typeparam>
        /// <param name="value">The color value to convert from.</param>
        void From8Bit<TColor>(TColor value) where TColor : IRGB<byte>;

        /// <summary>
        /// Populates the current color by converting from another 16-bit RGB or RGBA color type.
        /// </summary>
        /// <typeparam name="TColor">The source color type, which must implement <see cref="IRGB{T}"/> with 16-bit channels.</typeparam>
        /// <param name="value">The color value to convert from.</param>
        void From16Bit<TColor>(TColor value) where TColor : IRGB<ushort>;

    }
}
