using System;

namespace ZoDream.Shared.Drawing
{
    public class AY88 : IBufferDecoder, IBufferEncoder
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
                output[outputOffset] = data[offset];
                output[outputOffset + 1] = data[offset + 1];
                output[outputOffset + 2] = data[offset + 1];
                output[outputOffset + 3] = data[offset + 1];
            }
            return size * 4;
        }

        public byte[] Encode(ReadOnlySpan<byte> data, int width, int height)
        {
            var buffer = new byte[height * width * 2];
            for (int i = 0; i < height * width * 2; i += 2)
            {
                int index = 2 * i;
                buffer[i] = data[index];
                buffer[i + 1] = data[index + 3];
            }
            return buffer;
        }
    }
}
