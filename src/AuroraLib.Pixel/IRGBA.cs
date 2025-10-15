using System;
using System.Numerics;

namespace AuroraLib.Pixel
{
    /// <summary>
    /// Represents an RGBA (Red, Green, Blue, Alpha) color structure.
    /// </summary>
    /// <typeparam name="TValue">The numeric type representing the precision or range of the RGBA components.</typeparam>
    public interface IRGBA<TValue> : IRGB<TValue>, IAlpha<TValue> where TValue : unmanaged, IEquatable<TValue>, IComparable<TValue>, IFormattable
#if NET8_0_OR_GREATER
        , ISpanFormattable, IMinMaxValue<TValue>
#elif NET6_0_OR_GREATER
        , ISpanFormattable
#endif
    {
        /// <inheritdoc  cref="IAlpha{TValue}.A"/>
        new TValue A { get; set; }
    }
}
