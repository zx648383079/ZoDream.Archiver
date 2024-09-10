using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.Shared.Drawing
{
    public static class ColorNumerics
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte From16BitTo8Bit(ushort code) => (byte)(((code * 255) + 32895) >> 16);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort From8BitTo16Bit(byte code) => (ushort)(code * 257);

        public static ushort From16BitToShort(byte a, byte b) => (ushort)((a << 8) | b);

        public static uint RotateRight(uint value, int count)
        {
            return value >> count | value << 32 - count;
        }
        public static int Sum(Vector3 a)
        {
            return (int)(a.X + a.Y + a.Z);
        }

        public static int Sum(Vector4 a)
        {
            return (int)(a.X + a.Y + a.Z + a.W);
        }
    }
}
