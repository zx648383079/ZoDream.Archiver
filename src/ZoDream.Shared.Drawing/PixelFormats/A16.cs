using System;
using System.Buffers.Binary;

namespace ZoDream.Shared.Drawing
{
    public class A16 : IBufferDecoder
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
                var offset = i * 2;
                output[index] = 0xFF;
                output[index + 1] = 0xFF;
                output[index + 2] = 0xFF;
                output[index + 3] = ColorConverter.From16BitTo8Bit(
                    BinaryPrimitives.ReadUInt16BigEndian(data[offset..]));
            }
            return size * 4;
        }
    }
}
