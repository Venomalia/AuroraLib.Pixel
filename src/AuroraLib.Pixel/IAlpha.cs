using System;
using System.Numerics;

namespace AuroraLib.Pixel
{
    public interface IAlpha<TValue> : IColor where TValue : unmanaged, IEquatable<TValue>
    {
        /// <summary>
        /// The alpha (transparency) component of the color.
        /// </summary>
        TValue A { get; set; }
    }
}
