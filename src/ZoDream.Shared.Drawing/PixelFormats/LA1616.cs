using System;
using System.Buffers.Binary;

namespace ZoDream.Shared.Drawing
{
    public class LA1616 : IBufferDecoder
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
                var index = i * 4;
                var l = ColorConverter.From16BitTo8Bit(BinaryPrimitives.ReadUInt16BigEndian(data[index..]));
                output[index] = l;
                output[index + 1] = l;
                output[index + 2] = l;
                output[index + 3] = ColorConverter.From16BitTo8Bit(
                    BinaryPrimitives.ReadUInt16BigEndian(data[(index + 2)..]));
            }
            return size * 4;
        }
    }
}
