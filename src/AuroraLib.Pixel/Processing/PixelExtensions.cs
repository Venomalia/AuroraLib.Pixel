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
