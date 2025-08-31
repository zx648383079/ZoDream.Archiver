using System;
using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace ZoDream.Shared.Drawing
{
    public class RG1616 : IBufferDecoder
    {
        private static readonly Vector2 Max = new(ushort.MaxValue);

        public byte[] Decode(ReadOnlySpan<byte> data, int width, int height)
        {
            var buffer = new byte[width * height * 4];
            Decode(data, width, height, buffer);
            return buffer;
        }

        public int Decode(ReadOnlySpan<byte> data, int width, int height, Span<byte> output)
        {
            var size = width * height;
            for (var i = 0; i < size; i++)
            {
                var index = i * 4;
                var packed = Pack(new(BinaryPrimitives.ReadUInt16BigEndian(data[index..]),
                    BinaryPrimitives.ReadUInt16BigEndian(data[(index+2)..])));
                output[index] = ColorConverter.From16BitTo8Bit((ushort)(packed & 0xFFFF));
                output[index + 1] = ColorConverter.From16BitTo8Bit((ushort)(packed >> 16));
                output[index + 2] = byte.MinValue;
                output[index + 3] = byte.MaxValue;
            }
            return size * 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static uint Pack(Vector2 vector)
        {
            vector = Vector2.Clamp(vector, Vector2.Zero, Vector2.One) * Max;
            return (uint)(((int)Math.Round(vector.X) & 0xFFFF) | (((int)Math.Round(vector.Y) & 0xFFFF) << 16));
        }
    }
}
