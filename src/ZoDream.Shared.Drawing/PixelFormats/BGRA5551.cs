using System;
using System.Buffers.Binary;

namespace ZoDream.Shared.Drawing
{
    internal class BGRA5551 : IBufferDecoder
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
                var offset = i * 2;
                var outputOffset = i * 4;
                var temp = BinaryPrimitives.ReadUInt16BigEndian(data[offset..]);
                output[outputOffset] = (byte)(((temp >> 10) & 0x1F) / 31F);
                output[outputOffset + 1] = (byte)(((temp >> 5) & 0x1F) / 31F);
                output[outputOffset + 2] = (byte)(((temp >> 0) & 0x1F) / 31F);
                output[outputOffset + 3] = (byte)((temp >> 15) & 0x01);
            }
            return size * 4;
        }

    }
}
