using System;

namespace AuroraLib.Pixel
{
    public interface IAlpha<TValue> : IColor where TValue : unmanaged, IEquatable<TValue>, IComparable<TValue>
    {
        /// <summary>
        /// The alpha (transparency) component of the color.
        /// </summary>
        TValue A { get; set; }
    }
}
