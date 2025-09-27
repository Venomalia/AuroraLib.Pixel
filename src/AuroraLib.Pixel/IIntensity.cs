using System;
using System.Numerics;

namespace AuroraLib.Pixel
{
    /// <summary>
    /// Represents an intensity value, typically used for grayscale or single-value color representation.
    /// </summary>
    /// <typeparam name="TValue">The numeric type representing the precision or range of the  intensity value.</typeparam>
    public interface IIntensity<TValue> : IColor where TValue : unmanaged, IEquatable<TValue>, IComparable<TValue>, IConvertible
#if NET8_0_OR_GREATER
        , ISpanFormattable, IMinMaxValue<TValue>
#elif NET6_0_OR_GREATER
        , ISpanFormattable
#endif
    {
        /// <summary>
        /// The intensity value, representing the brightness or single color component value.
        /// </summary>
        TValue I { get; set; }
    }
}
