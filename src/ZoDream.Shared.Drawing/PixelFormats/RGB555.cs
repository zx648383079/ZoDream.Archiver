using System;
using System.Buffers.Binary;

namespace ZoDream.Shared.Drawing
{
    public class RGB555 : IBufferDecoder
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
                var packed = BinaryPrimitives.ReadUInt16BigEndian(data[offset..]);
                output[outputOffset] = (byte)(packed & 0x7C00);
                output[outputOffset + 1] = (byte)(packed & 0x03E0);
                output[outputOffset + 2] = (byte)(packed & 0x001F);
                output[outputOffset + 3] = byte.MaxValue;
            }
            return size * 4;
        }

    }
}
