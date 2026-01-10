using AuroraLib.Pixel;
using AuroraLib.Pixel.PixelFormats;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Numerics;

namespace PixelTest
{

    [TestClass]
    public class FloatScaleHelperTest
    {
#if DEBUG
        [TestMethod]
        public void ScaleBits_ByteToShort_MaxValue()
        {
            short back = ScaleHelper<short>.ScaleBits(byte.MaxValue);
            Assert.AreEqual(back, short.MaxValue);
        }

        [TestMethod]
        public void ScaleBits_ByteToShort_MinValue()
        {
            short back = ScaleHelper<short>.ScaleBits(byte.MinValue);
            Assert.AreEqual(back, short.MinValue);
        }

        [TestMethod]
        public void ScaleBits_ByteToUint_MaxValue()
        {
            uint back = ScaleHelper<uint>.ScaleBits(byte.MaxValue);
            Assert.AreEqual(back, uint.MaxValue);
        }

        [TestMethod]
        public void ScaleBits_ByteToUint_MinValue()
        {
            uint back = ScaleHelper<uint>.ScaleBits(byte.MinValue);
            Assert.AreEqual(back, uint.MinValue);
        }

        [TestMethod]
        public void ScaleBits_ByteToIInt_MaxValue()
        {
            int back = ScaleHelper<int>.ScaleBits(byte.MaxValue);
            Assert.AreEqual(back, int.MaxValue);
        }

        [TestMethod]
        public void ScaleBits_ByteToInt_MinValue()
        {
            int back = ScaleHelper<int>.ScaleBits(byte.MinValue);
            Assert.AreEqual(back, int.MinValue);
        }

        [TestMethod]
        public void ScaleBits_IntToUint_MaxValue()
        {
            uint back = ScaleHelper<uint>.ScaleBits(int.MaxValue);
            Assert.AreEqual(back, uint.MaxValue);
        }

        [TestMethod]
        public void ScaleBits_IntToUint_MinValue()
        {
            uint back = ScaleHelper<uint>.ScaleBits(int.MinValue);
            Assert.AreEqual(back, uint.MinValue);
        }

        #region MinMitMax
        private static void Assert_MinMitMax(float min, float mit, float max)
        {
            Assert.AreEqual(0f, min);
            Assert.IsTrue(mit > 0.49f && mit < 0.51f);
            Assert.AreEqual(1f, max);
        }

        [TestMethod]
        public void ToScaledFloat_MinMitMax_Byte()
        {
            float fMin = ScaleHelper<byte>.ToScaledFloat(byte.MinValue);
            float fMid = ScaleHelper<byte>.ToScaledFloat(byte.MaxValue / 2);
            float fMax = ScaleHelper<byte>.ToScaledFloat(byte.MaxValue);

            Assert_MinMitMax(fMin, fMid, fMax);
        }

        [TestMethod]
        public void ToScaledFloat_MinMitMax_Sbyte()
        {
            float fMin = ScaleHelper<sbyte>.ToScaledFloat(sbyte.MinValue);
            float fMid = ScaleHelper<sbyte>.ToScaledFloat(0);
            float fMax = ScaleHelper<sbyte>.ToScaledFloat(sbyte.MaxValue);

            Assert_MinMitMax(fMin, fMid, fMax);
        }

        [TestMethod]
        public void ToScaledFloat_MinMitMax_Ushort()
        {
            float fMin = ScaleHelper<ushort>.ToScaledFloat(ushort.MinValue);
            float fMid = ScaleHelper<ushort>.ToScaledFloat(ushort.MaxValue / 2);
            float fMax = ScaleHelper<ushort>.ToScaledFloat(ushort.MaxValue);

            Assert_MinMitMax(fMin, fMid, fMax);
        }

        [TestMethod]
        public void ToScaledFloat_MinMitMax_Short()
        {
            float fMin = ScaleHelper<short>.ToScaledFloat(short.MinValue);
            float fMid = ScaleHelper<short>.ToScaledFloat(0);
            float fMax = ScaleHelper<short>.ToScaledFloat(short.MaxValue);

            Assert_MinMitMax(fMin, fMid, fMax);
        }

        [TestMethod]
        public void ToScaledFloat_MinMitMax_Uint()
        {
            float fMin = ScaleHelper<uint>.ToScaledFloat(uint.MinValue);
            float fMid = ScaleHelper<uint>.ToScaledFloat(uint.MaxValue / 2);
            float fMax = ScaleHelper<uint>.ToScaledFloat(uint.MaxValue);

            Assert_MinMitMax(fMin, fMid, fMax);
        }

        [TestMethod]
        public void ToScaledFloat_MinMitMax_Int()
        {
            float fMin = ScaleHelper<int>.ToScaledFloat(int.MinValue);
            float fMid = ScaleHelper<int>.ToScaledFloat(0);
            float fMax = ScaleHelper<int>.ToScaledFloat(int.MaxValue);

            Assert_MinMitMax(fMin, fMid, fMax);
        }

        [TestMethod]
        public void ToScaledFloat_MinMitMax_Float()
        {
            float fMin = ScaleHelper<float>.ToScaledFloat(0);
            float fMid = ScaleHelper<float>.ToScaledFloat(0.5f);
            float fMax = ScaleHelper<float>.ToScaledFloat(1);

            Assert_MinMitMax(fMin, fMid, fMax);
        }

        [TestMethod]
        public void ToScaledFloat_MinMitMax_Half()
        {
            float fMin = ScaleHelper<Half>.ToScaledFloat((Half)0);
            float fMid = ScaleHelper<Half>.ToScaledFloat((Half)0.5f);
            float fMax = ScaleHelper<Half>.ToScaledFloat((Half)1);

            Assert_MinMitMax(fMin, fMid, fMax);
        }
        #endregion

        private static void Assert_Fullrange<TValue>(TValue min, TValue max) where TValue : unmanaged, IEquatable<TValue>, IComparable<TValue>
#if NET8_0_OR_GREATER
, ISpanFormattable, IMinMaxValue<TValue>, INumber<TValue>
#endif
        {
            TValue bMin = ScaleHelper<TValue>.FromScaledFloat(0f);
            TValue bMax = ScaleHelper<TValue>.FromScaledFloat(1f);

            Assert.AreEqual(min, bMin);
            Assert.AreEqual(max, bMax);
        }

        [TestMethod]
        public void FromScaledFloat_Fullrange_Sbyte()
        {
            Assert_Fullrange(sbyte.MinValue, sbyte.MaxValue);
        }

        [TestMethod]
        public void FromScaledFloat_Fullrange_Ushort()
        {
            Assert_Fullrange(ushort.MinValue, ushort.MaxValue);
        }

        [TestMethod]
        public void FromScaledFloat_Fullrange_Short()
        {
            Assert_Fullrange(short.MinValue, short.MaxValue);
        }

        [TestMethod]
        public void FromScaledFloat_Fullrange_Uint()
        {
            Assert_Fullrange(uint.MinValue, uint.MaxValue);
        }

        [TestMethod]
        public void FromScaledFloat_Fullrange_Int()
        {
            Assert_Fullrange(int.MinValue, int.MaxValue);
        }

        #region Roundtrip

        private static void AssertRoundtrip<TValue>(float threshold, float step) where TValue : unmanaged, IEquatable<TValue>, IComparable<TValue>
#if NET8_0_OR_GREATER
        , ISpanFormattable, IMinMaxValue<TValue>, INumber<TValue>
#endif
        {
            for (float v = 0f; v <= 1f; v += step)
            {
                TValue b = ScaleHelper<TValue>.FromScaledFloat(v);
                float back = ScaleHelper<TValue>.ToScaledFloat(b);

                Assert.AreEqual(back, v, threshold);
            }
        }

        [TestMethod]
        public void FromScaledFloat_ShouldRoundtrip_Byte()
        {
            float threshold = 1f / byte.MaxValue;
            AssertRoundtrip<byte>(threshold, threshold);
        }

        [TestMethod]
        public void FromScaledFloat_ShouldRoundtrip_Sbyte()
        {
            float threshold = 1f / byte.MaxValue;
            AssertRoundtrip<sbyte>(threshold, threshold);
        }

        [TestMethod]
        public void FromScaledFloat_ShouldRoundtrip_Ushort()
        {
            float threshold = 1f / ushort.MaxValue;
            AssertRoundtrip<ushort>(threshold, 0.0012f);
        }

        [TestMethod]
        public void FromScaledFloat_ShouldRoundtrip_Short()
        {
            float threshold = 1f / ushort.MaxValue;
            AssertRoundtrip<short>(threshold, 0.0012f);
        }

        [TestMethod]
        public void FromScaledFloat_ShouldRoundtrip_Uint()
        {
            float threshold = 1f / uint.MaxValue;
            AssertRoundtrip<uint>(threshold, 0.00012f);
        }

        [TestMethod]
        public void FromScaledFloat_ShouldRoundtrip_Int()
        {
            float threshold = 1f / uint.MaxValue;
            AssertRoundtrip<int>(threshold, 0.00012f);
        }

        #endregion
        [TestMethod]
        public void Roundtrip_WithEdgeTolerance_Uint()
        {
            uint pass = 1024;
            for (uint i = 0; i < pass; i++)
            {
                float f = ScaleHelper<uint>.ToScaledFloat(i);
                uint back = ScaleHelper<uint>.FromScaledFloat(f);
                Assert.AreEqual(i, back);
            }

            for (uint i = uint.MaxValue; i > uint.MaxValue - pass; i--)
            {
                float f = ScaleHelper<uint>.ToScaledFloat(i);
                uint back = ScaleHelper<uint>.FromScaledFloat(f);
                Assert.AreEqual(i, back, 128);
            }
        }

        [TestMethod]
        public void Roundtrip_WithEdgeTolerance_Int()
        {
            uint pass = 1024;
            for (int i = int.MinValue; i < int.MinValue + pass; i++)
            {
                float f = ScaleHelper<int>.ToScaledFloat(i);
                int back = ScaleHelper<int>.FromScaledFloat(f);
                Assert.AreEqual(i, back);
            }

            for (int i = int.MaxValue; i > int.MaxValue - pass; i--)
            {
                float f = ScaleHelper<int>.ToScaledFloat(i);
                int back = ScaleHelper<int>.FromScaledFloat(f);
                Assert.AreEqual(i, back, 128);
            }
        }
#endif
    }
}
