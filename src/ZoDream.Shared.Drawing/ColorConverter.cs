using System;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace ZoDream.Shared.Drawing
{
    public static class ColorConverter
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte From16BitTo8Bit(ushort code) => (byte)(((code * 255) + 32895) >> 16);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort From8BitTo16Bit(byte code) => (ushort)(code * 257);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte FromHalfToByte(ReadOnlySpan<byte> buffer)
        {
            return (byte)Math.Round((float)BitConverter.ToHalf(buffer) * 255f);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte FromFloatToByte(ReadOnlySpan<byte> buffer)
        {
            return (byte)Math.Round(BitConverter.ToSingle(buffer) * 255f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint RotateRight(uint value, int count)
        {
            return value >> count | value << 32 - count;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Sum(Vector3 a)
        {
            return (int)(a.X + a.Y + a.Z);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Sum(Vector4 a)
        {
            return (int)(a.X + a.Y + a.Z + a.W);
        }

        public static byte[] SplitByte(ReadOnlySpan<byte> input, int start, out int length, 
            params int[] chunks)
        {
            var sum = chunks.Sum();
            length = (int)Math.Ceiling((double)sum / 8);
            var total = 0u;
            for (var i = 0; i < length; i++)
            {
                total = (total << 8) + input[start + i];
            }
            var res = new byte[chunks.Length];
            for (var i = 0; i < chunks.Length; i++)
            {
                sum -= chunks[i];
                res[i] = (byte)((total >> sum) & MaxValue(chunks[i]));
            }
            return res;
        }

        private static uint MaxValue(int size)
        {
            var total = 0u;
            for (var i = 0; i < size; i++)
            {
                total = (total << 1) + 1;
            }
            return total;
        }
    }
}
