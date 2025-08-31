using System;

namespace ZoDream.Shared.Drawing
{
    public class RGBA4444 : IBufferDecoder
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
                output[outputOffset + 2] = (byte)((data[offset + 1] & 0x0F) << 4);
                output[outputOffset + 3] = (byte)(data[offset + 1] & 0xF0);
                output[outputOffset + 0] = (byte)((data[offset] & 0x0F) << 4);
                output[outputOffset + 1] = (byte)(data[offset] & 0xF0);
            }
            return size * 4;
        }
    }
}
