using System;

namespace AuroraLib.Pixel
{
    /// <summary>
    /// Represents a YUV color model with generic component type.
    /// </summary>
    /// <typeparam name="TValue">The numeric type representing the precision or range of the YUV components.</typeparam>
    public interface IYUV<TValue> : IColor where TValue : unmanaged, IEquatable<TValue>, IComparable<TValue>
    {
        /// <summary>
        /// The luminance (brightness) component.
        /// </summary>
        TValue Y { get; set; }

        /// <summary>
        /// The chrominance-blue component (Cb), representing the blue color deviation.
        /// </summary>
        TValue U { get; set; }

        /// <summary>
        /// The chrominance-red component (Cr), representing the red color deviation.
        /// </summary>
        TValue V { get; set; }
    }
}
