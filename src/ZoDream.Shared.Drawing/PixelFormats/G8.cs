using System;

namespace ZoDream.Shared.Drawing
{
    public class G8 : IBufferDecoder
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
                output[index] = byte.MaxValue;
                output[index + 1] = byte.MaxValue;
                output[index + 2] = data[i];
                output[index + 3] = byte.MaxValue;
            }
            return size * 4;
        }
    }
}
