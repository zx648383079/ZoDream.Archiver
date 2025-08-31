using System;
using System.Buffers.Binary;

namespace ZoDream.Shared.Drawing
{
    public class R16 : IBufferDecoder
    {
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
                var l = BinaryPrimitives.ReadUInt16BigEndian(data[(i * 2)..]);
                var b = ColorConverter.From16BitTo8Bit(l);
                var index = i * 4;
                output[index] = b;
                output[index + 1] = 0;
                output[index + 2] = 0;
                output[index + 3] = byte.MaxValue;
            }
            return size * 4;
        }
    }
}
