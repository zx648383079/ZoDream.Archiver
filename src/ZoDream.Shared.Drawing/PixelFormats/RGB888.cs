using System;

namespace ZoDream.Shared.Drawing
{
    public class RGB888 : IBufferDecoder
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
                var offset = i * 3;
                output[index] = data[offset];
                output[index + 1] = data[offset + 1];
                output[index + 2] = data[offset + 2];
                output[index + 3] = byte.MaxValue;
            }
            return size * 4;
        }
    }
}
