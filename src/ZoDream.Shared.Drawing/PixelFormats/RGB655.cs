using System;

namespace ZoDream.Shared.Drawing
{
    internal class RGB655 : IBufferDecoder
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
                var res = ColorConverter.SplitByte(data, i * 2, out _, 6, 5, 5);
                output[index] = res[0];
                output[index + 1] = res[1];
                output[index + 2] = res[2];
                output[index + 3] = byte.MaxValue;
            }
            return size * 4;
        }

    }
}
