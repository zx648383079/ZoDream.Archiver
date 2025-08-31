using System;

namespace ZoDream.Shared.Drawing
{
    public class LA88 : IBufferDecoder
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
                var index = i * 4;
                output[index] = data[offset];
                output[index + 1] = data[offset];
                output[index + 2] = data[offset];
                output[index + 3] = data[offset + 1];
            }
            return size * 4;
        }
    }
}
