using System;
using System.Numerics;

namespace AuroraLib.Pixel
{
    /// <summary>
    /// Represents a basic RGB (Red, Green, Blue) color structure.
    /// </summary>
    /// <typeparam name="TValue">The numeric type representing the precision or range of the RGB components.</typeparam>
    public interface IRGB<TValue> : IColor where TValue : unmanaged, IEquatable<TValue>, IComparable<TValue>
#if NET8_0_OR_GREATER
        , ISpanFormattable, IMinMaxValue<TValue>
#elif NET6_0_OR_GREATER
        , ISpanFormattable
#endif
    {
        /// <summary>
        /// The red component of the color.
        /// </summary>
        TValue R { get; set; }

        /// <summary>
        /// The green component of the color.
        /// </summary>
        TValue G { get; set; }

        /// <summary>
        /// The blue component of the color.
        /// </summary>
        TValue B { get; set; }

        /// <summary>
        /// Get the alpha (transparency) component of the color.
        /// </summary>
        TValue A { get; }

    }
}
