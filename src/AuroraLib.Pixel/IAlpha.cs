using System;

namespace AuroraLib.Pixel
{
    /// <inheritdoc cref="IAlpha{TValue}"/>
    public interface IAlpha : IColor
    {
        /// <summary>
        /// Gets or sets the normalized alpha value of the color component.
        /// The value is in the range of [0, 1].
        /// </summary>
        float A { get; set; }
    }

    public interface IAlpha<TValue> : IAlpha where TValue : unmanaged, IEquatable<TValue>, IComparable<TValue>, IConvertible
#if NET8_0_OR_GREATER
        , ISpanParsable<TValue>
#endif

    {
        /// <summary>
        /// The alpha (transparency) component of the color.
        /// </summary>
        new TValue A { get; set; }
    }
}
