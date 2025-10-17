using System.Numerics;

namespace AuroraLib.Pixel.PixelFormats.Wrapper
{
    /// <summary>
    /// Represents a signed normalized color format wrapper.
    /// The underlying storage is interpreted as SNORM instead of its original UNORM format.
    /// </summary>
    /// <typeparam name="TColor">The underlying UNORM color type.</typeparam>
    public struct SNORM<TColor> : IColor<SNORM<TColor>> where TColor : unmanaged, IColor<TColor>
    {
        /// <inheritdoc/>
        public PixelFormatInfo FormatInfo
        {
            get
            {
                var i = Value.FormatInfo;
                return new PixelFormatInfo(i.BitsPerPixel, i.RedChannelInfo.BitDepth, i.RedChannelInfo.Shift, i.GreenChannelInfo.BitDepth, i.GreenChannelInfo.Shift, i.BlueChannelInfo.BitDepth, i.BlueChannelInfo.Shift, i.AlphaChannelInfo.BitDepth, i.AlphaChannelInfo.Shift, i.ColorSpace, PixelFormatInfo.ChannelType.Signed);
            }
        }

        private TColor Value;

        /// <summary>
        /// Static constructor initializes offset and correction vectors.
        /// </summary>
        static SNORM()
        {
            var info = default(TColor).FormatInfo;
            Offset = new Vector4();
            Correction = new Vector4();
            (Offset.X, Correction.X) = GetOandC(info.RedChannelInfo.BitDepth);
            (Offset.Y, Correction.Y) = GetOandC(info.GreenChannelInfo.BitDepth);
            (Offset.Z, Correction.Z) = GetOandC(info.BlueChannelInfo.BitDepth);
            (Offset.W, Correction.W) = GetOandC(info.AlphaChannelInfo.BitDepth);

            static (float offset, float full) GetOandC(int bits)
            {
                if (bits <= 0) return (0f, 0f);
                if (bits >= 20) return (0.5f, -1f);

                float maxVal = (1 << bits) - 1f;
                float offset = (1 << bits - 1) / maxVal;
                float full = (1 << bits) / maxVal;
                return (offset, -full);
            }
        }

        private static readonly Vector4 Offset;
        private static readonly Vector4 Correction;

        /// <inheritdoc/>
        public float Mask
        {
            readonly get => Offset.W == 0 ? Help.BT709Luminance(ToScaledVector4()) : ToScaledVector4().W;
            set
            {
                var v = ToScaledVector4();
                if (Offset.W == 0)
                    v.X = v.Y = v.Z = value;
                else
                    v.W = value;
                FromScaledVector4(v);
            }
        }

        /// <inheritdoc/>
        public bool Equals(SNORM<TColor> other) => Value.Equals(other.Value);

        /// <inheritdoc/>
        public void From16Bit<TColor>(TColor value) where TColor : IRGB<ushort>
            => FromScaledVector4(value.ToScaledVector4());

        /// <inheritdoc/>
        public void From8Bit<TColor>(TColor value) where TColor : IRGB<byte>
            => FromScaledVector4(value.ToScaledVector4());

        /// <inheritdoc/>
        public void ToRGBA<TColor>(ref TColor value) where TColor : IRGBA<byte>
            => value.FromScaledVector4(ToScaledVector4());

        /// <inheritdoc/>
        public void FromScaledVector4(Vector4 vector)
            => Value.FromScaledVector4(FixVectorApprox(vector));

        /// <inheritdoc/>
        public readonly Vector4 ToScaledVector4()
            => FixVectorApprox(Value.ToScaledVector4());

        private static Vector4 FixVectorApprox(Vector4 v)
        {
            const float threshold = 0.5f;
            Vector4 mask = new Vector4(
                v.X >= threshold ? 1f : 0f,
                v.Y >= threshold ? 1f : 0f,
                v.Z >= threshold ? 1f : 0f,
                v.W >= threshold ? 1f : 0f
            );
            Vector4 corrected = v + Offset + mask * Correction;
            return Vector4.Clamp(corrected, Vector4.Zero, Vector4.One);
        }
    }
}
