using System;
using System.Buffers.Binary;

namespace ZoDream.Shared.Drawing
{
    public class ARGB1555 : IBufferDecoder
    {
        public byte[] Decode(ReadOnlySpan<byte> data, int width, int height)
        {
            var buffer = new byte[width * height * 4];
            Decode(data, width, height, buffer);
            return buffer;
        }

        public int Decode(ReadOnlySpan<byte> data, int width, int height, Span<byte> output)
        {
            var size = width * height * 2;
            for (int i = 0; i < size; i += 2)
            {
                var temp = BinaryPrimitives.ReadUInt16BigEndian(data[i..]);
                output[i * 2] = (byte)(temp & 0x1F);
                output[i * 2 + 1] = (byte)((temp >> 5) & 0x3F);
                output[i * 2 + 2] = (byte)((temp >> 11) & 0x1F);
                output[i * 2 + 3] = 0xFF;
            }
            return size * 2;
        }
    }
}
