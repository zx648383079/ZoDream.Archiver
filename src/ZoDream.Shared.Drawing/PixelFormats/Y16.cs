using System;

namespace ZoDream.Shared.Drawing
{
    public class Y16 : IBufferDecoder
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
                // 16 bit color, but stored in 8 bits, precision loss, we can use the most important byte and truncate the rest for now.
                // ushort color = (ushort)((data[i * 2]) | (data[i * 2 + 1] << 8));
                var index = i * 4;
                output[index] = data[i * 2 + 1];
                output[index + 1] = data[i * 2 + 1];
                output[index + 2] = data[i * 2 + 1];
                output[index + 3] = byte.MaxValue;
            }
            return size * 4;
        }

    }
}
