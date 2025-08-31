using System;

namespace ZoDream.Shared.Drawing
{
    public class Y8 : IBufferDecoder, IBufferEncoder
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
                output[index] = 0xFF;
                output[index + 1] = data[i];
                output[index + 2] = data[i];
                output[index + 3] = data[i];
            }
            return size * 4;
        }

        public byte[] Encode(ReadOnlySpan<byte> data, int width, int height)
        {
            var buffer = new byte[height * width];
            for (int i = 0; i < (height * width); i++)
            {
                int index = i * 4;
                buffer[i] = data[index + 1];
            }
            return buffer;
        }
    }
}
