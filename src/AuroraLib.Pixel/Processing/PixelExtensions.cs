using AuroraLib.Pixel.PixelFormats;
using AuroraLib.Pixel.PixelProcessor;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AuroraLib.Pixel.Processing
{
    public static class PixelExtensions
    {

        /// <summary>
        /// Converts the specified color to a hexadecimal string representation.
        /// </summary>
        /// <typeparam name="TColor">The unmanaged color type implementing <see cref="IColor"/>.</typeparam>
        /// <param name="color">The color value to convert.</param>
        /// <param name="format">The hexadecimal color format to use.</param>
        /// <param name="prefix"><c>true</c> to prefix the result with '#'; otherwise, <c>false</c>.</param>
        /// <returns>A hexadecimal string representing the color.</returns>
        public static string ToHex<TColor>(this TColor color, HexColorFormat format = HexColorFormat.RRGGBBAA, bool prefix = true) where TColor : unmanaged, IColor
        {
            int totalLen = (int)format + (prefix ? 1 : 0);
            Span<char> buffer = stackalloc char[totalLen];

            if (!color.TryFormatHex(buffer, format, prefix))
                throw new InvalidOperationException("Invalid hex format.");

            return buffer.ToString();
        }

        /// <summary>
        /// Attempts to format the specified color as a hexadecimal string into the provided character span.
        /// </summary>
        /// <typeparam name="TColor">The unmanaged color type implementing <see cref="IColor"/>.</typeparam>
        /// <param name="color">The color value to format.</param>
        /// <param name="dest">The destination span that receives the hexadecimal characters.</param>
        /// <param name="format">The hexadecimal color format to use.</param>
        /// <param name="prefix"><c>true</c> to prefix the result with '#'; otherwise, <c>false</c>.</param>
        /// <returns><c>true</c> if the color was successfully formatted; otherwise, <c>false</c>.</returns>
        public static bool TryFormatHex<TColor>(this TColor color, Span<char> dest, HexColorFormat format = HexColorFormat.RRGGBBAA, bool prefix = true) where TColor : unmanaged, IColor
        {
            // Hex digits lookup table for fast conversion
            const string Hex = "0123456789ABCDEF";

            // Write '#' if requested, and slice the span.
            if (prefix)
            {
                dest[0] = '#';
                dest = dest.Slice(1);
            }

            // Ensure the destination span is large enough for the requested hex format.
            if (dest.Length < (int)format)
                return false;

            RGBA<byte> rgba = default;
            color.ToRGBA(ref rgba);

            // Format the color depending on the requested hex format.
            switch (format)
            {
                case HexColorFormat.RGB:
                case HexColorFormat.RGBA:
                    // For short format, use only the upper 4 bits of each channel
                    dest[0] = Hex[rgba.R >> 4];
                    dest[1] = Hex[rgba.G >> 4];
                    dest[2] = Hex[rgba.B >> 4];

                    // Include alpha if requested
                    if (format == HexColorFormat.RGBA)
                        dest[3] = Hex[rgba.A >> 4];

                    return true;
                case HexColorFormat.RRGGBB:
                case HexColorFormat.RRGGBBAA:
                    // Full byte format: write each channel as two hex digits
                    WriteHexByte(rgba.R, dest.Slice(0, 2));
                    WriteHexByte(rgba.G, dest.Slice(2, 2));
                    WriteHexByte(rgba.B, dest.Slice(4, 2));

                    // Include alpha if requested
                    if (format == HexColorFormat.RRGGBBAA)
                        WriteHexByte(rgba.A, dest.Slice(6, 2));

                    return true;
                default:
                    return false;
            }

            static void WriteHexByte(byte value, Span<char> dest)
            {
                dest[0] = Hex[value >> 4];
                dest[1] = Hex[value & 0xF];
            }
        }

#if NET5_0_OR_GREATER
        public static bool TryParseHex<TColor>(this ref TColor result, ReadOnlySpan<char> hex) where TColor : unmanaged, IColor
#else
        public static bool TryParseHex<TColor>(this ref TColor result, string? hex) where TColor : unmanaged, IColor
#endif
            => TryParseHex(hex, out result);

#if NET5_0_OR_GREATER
        public static void ParseHex<TColor>(this ref TColor result, ReadOnlySpan<char> hex) where TColor : unmanaged, IColor
#else
        public static void ParseHex<TColor>(this ref TColor result, string? hex) where TColor : unmanaged, IColor
#endif
        {
            if (!TryParseHex(hex, out result))
                throw new FormatException($"The value '{hex}' is not a valid hex color.");
        }

        /// <summary>
        /// Parses a 3,4,6 or 8 digit hexadecimal RGBA color code into an <typeparamref name="TColor"/> value.
        /// </summary>
        /// <param name="hex">The hexadecimal input.</param>
        /// <param name="result">The parsed <typeparamref name="TColor"/> value if successful, otherwise the default.</param>
        /// <returns><c>true</c> if parsing succeeded; otherwise, <c>false</c>.</returns>
#if NET5_0_OR_GREATER
        public static bool TryParseHex<TColor>(ReadOnlySpan<char> hex, out TColor result) where TColor : unmanaged, IColor
        {
            result = default;
            if (hex.IsEmpty)
                return false;

            if (hex[0] == '#')
                hex = hex[1..];
#else
        public static bool TryParseHex<TColor>(string? hex, out TColor result) where TColor : unmanaged, IColor
        {
            result = default;
            if (string.IsNullOrEmpty(hex))
                return false;

            if (hex![0] == '#')
                hex = hex.Substring(1);
#endif

            if (hex.Length == 3 || hex.Length == 4)
            {
                // Handle short RGBA16 format (3 or 4 digits)
                if (ushort.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ushort packedValue))
                {
                    // if 3-digit RGB: shift left 4 bits and set Alpha to max (0xF)
                    RGBA16 rgba16 = hex.Length == 3 ? (ushort)((packedValue << 4) | 0xF) : packedValue;
                    result.From8Bit(rgba16);
                    return true;
                }
                return false;
            }

            if (hex.Length == 6 || hex.Length == 8)
            {
                // Handle full 32-bit ABGR format (6 or 8 digits)
                if (uint.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint packedValue))
                {
                    // if 6-digit RGB: shift left 8 bits and set Alpha to max (0xFF)
                    ABGR<byte> rbga32 = hex.Length == 6 ? ((packedValue << 8) | 0xFF) : packedValue;
                    result.From8Bit(rbga32);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Converts each color of <typeparamref name="TFrom"/> to a color of <typeparamref name="TTo"/>.
        /// </summary>
        /// <typeparam name="TFrom">The source color type.</typeparam>
        /// <typeparam name="TTo">The target color type.</typeparam>
        /// <param name="sFrom">The source span of colors.</param>
        /// <param name="sTo">The destination span to receive the converted colors.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void To<TFrom, TTo>(this ReadOnlySpan<TFrom> sFrom, Span<TTo> sTo)
            where TFrom : unmanaged, IColor
            where TTo : unmanaged, IColor
        {
            if (sFrom.Length != sTo.Length)
                throw new ArgumentException("Source and destination spans must have the same length.");

            if (typeof(TFrom) == typeof(TTo))
            {
                MemoryMarshal.Cast<TFrom, TTo>(sFrom).CopyTo(sTo);
            }
            else if (typeof(TTo) == typeof(RGBA<byte>))
            {
                ToRGBA(sFrom, MemoryMarshal.Cast<TTo, RGBA<byte>>(sTo));
            }
            else if (typeof(TTo) == typeof(BGRA<byte>))
            {
                ToRGBA(sFrom, MemoryMarshal.Cast<TTo, BGRA<byte>>(sTo));
            }
            else if (typeof(TTo) == typeof(ARGB<byte>))
            {
                ToRGBA(sFrom, MemoryMarshal.Cast<TTo, ARGB<byte>>(sTo));
            }
            else if (typeof(TTo) == typeof(ABGR<byte>))
            {
                ToRGBA(sFrom, MemoryMarshal.Cast<TTo, ABGR<byte>>(sTo));
            }
            else if (typeof(TFrom) == typeof(RGBA<byte>))
            {
                ToFrom8Bit(MemoryMarshal.Cast<TFrom, RGBA<byte>>(sFrom), sTo);
            }
            else if (typeof(TFrom) == typeof(RGB<byte>))
            {
                ToFrom8Bit(MemoryMarshal.Cast<TFrom, RGB<byte>>(sFrom), sTo);
            }
            else if (typeof(TFrom) == typeof(BGRA<byte>))
            {
                ToFrom8Bit(MemoryMarshal.Cast<TFrom, BGRA<byte>>(sFrom), sTo);
            }
            else if (typeof(TFrom) == typeof(BGR<byte>))
            {
                ToFrom8Bit(MemoryMarshal.Cast<TFrom, BGR<byte>>(sFrom), sTo);
            }
            else
            {
                for (int i = 0; i < sFrom.Length; i++)
                {
                    sTo[i].FromScaledVector4(sFrom[i].ToScaledVector4());
                }
            }

            static void ToFrom8Bit<Trgb>(ReadOnlySpan<Trgb> sFrom, Span<TTo> sTo) where Trgb : unmanaged, IColor, IRGB<byte>
            {
                for (int i = 0; i < sFrom.Length; i++)
                {
                    sTo[i].From8Bit(sFrom[i]);
                }
            }
            static void ToRGBA<Trgba>(ReadOnlySpan<TFrom> sFrom, Span<Trgba> sTo) where Trgba : unmanaged, IColor, IRGBA<byte>
            {
                for (int i = 0; i < sFrom.Length; i++)
                {
                    sFrom[i].ToRGBA(ref sTo[i]);
                }
            }
        }

        /// <inheritdoc cref="To{TFrom, TTo}(Span{TFrom}, Span{TTo})"/>
        public static void To<TFrom, TTo>(this Span<TFrom> sFrom, Span<TTo> sTo)
            where TFrom : unmanaged, IColor
            where TTo : unmanaged, IColor
            => ((ReadOnlySpan<TFrom>)sFrom).To(sTo);

        /// <summary>
        /// Blends the colors from the <paramref name="source"/> span into the <paramref name="target"/> span using a specified <paramref name="blendMode"/>.
        /// </summary>
        /// <typeparam name="TFrom">The type of source colors, which must implement the IColor interface.</typeparam>
        /// <typeparam name="TTo">The type of target color values, which must also implement the IColor interface.</typeparam>
        /// <param name="target">The target span of color values that will be modified.</param>
        /// <param name="source">The source span of color values that will be used in the blending calculation.</param>
        /// <param name="blendMode">The blend function that defines how to blend the source and target spans.</param>
        /// <param name="intensity">The intensity of the blending.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Blend<TFrom, TTo>(this Span<TTo> target, ReadOnlySpan<TFrom> source, BlendModes.BlendFunction blendMode, float intensity = 1f)
            where TFrom : unmanaged, IColor
            where TTo : unmanaged, IColor
        {
            if (source.Length != target.Length)
                throw new ArgumentException("Source and destination spans must have the same length.");

            for (int i = 0; i < source.Length; i++)
            {
                target[i].FromScaledVector4(blendMode(target[i].ToScaledVector4(), source[i].ToScaledVector4(), intensity));
            }
        }

        /// <summary>
        /// Blends the colors from the <paramref name="source"/> span into the <paramref name="target"/> span using a specified <paramref name="blendMode"/> and a <paramref name="mask"/>.
        /// </summary>
        /// <typeparam name="TFrom">The type of source colors.</typeparam>
        /// <typeparam name="TTo">The type of target color values.</typeparam>
        /// <typeparam name="TMask">The type of mask values.</typeparam>
        /// <param name="target">The target span of color values that will be modified.</param>
        /// <param name="source">The source span of color values that will be used in the blending calculation.</param>
        /// <param name="mask">A span of mask values that determine the intensity of the blend operation.</param>
        /// <param name="blendMode">The blend function that defines how to blend the source and target spans.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Blend<TFrom, TTo, TMask>(this Span<TTo> target, ReadOnlySpan<TFrom> source, ReadOnlySpan<TMask> mask, BlendModes.BlendFunction blendMode)
            where TFrom : unmanaged, IColor
            where TTo : unmanaged, IColor
            where TMask : unmanaged, IColor
        {
            if (source.Length != target.Length || source.Length != mask.Length)
                throw new ArgumentException("Source, destination and mask spans must have the same length.");

            for (int i = 0; i < source.Length; i++)
            {
                target[i].FromScaledVector4(blendMode(target[i].ToScaledVector4(), source[i].ToScaledVector4(), mask[i].Mask));
            }
        }

    }
}
