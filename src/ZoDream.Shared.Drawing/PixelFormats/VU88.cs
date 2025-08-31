using System;

namespace ZoDream.Shared.Drawing
{
    public class VU88(bool swapXY = false) : IBufferDecoder, IBufferEncoder
    {
        public byte[] Decode(ReadOnlySpan<byte> data, int width, int height)
        {
            var buffer = new byte[width * height * 4];
            Decode(data, width, height, buffer);
            return buffer;
        }

        public int Decode(ReadOnlySpan<byte> data, int width, int height, Span<byte> output)
        {
            var size = width * height * 2;
            for (var i = 0; i < size; i+=2)
            {
                byte X = (byte)(data[i + 1] + 127);
                byte Y = (byte)(data[i + 0] + 127);

                output[i * 2] = 0xFF;
                output[(i * 2) + 1] = swapXY ? Y : X;
                output[(i * 2) + 2] = swapXY ? X : Y;
                output[(i * 2) + 3] = 0xFF;
            }
            return size * 2;
        }

        public byte[] Encode(ReadOnlySpan<byte> data, int width, int height)
        {
            var buffer = new byte[height * width * 2];
            for (int i = 0; i < height * width * 2; i += 2)
            {
                int index = 2 * i;
                buffer[i] = data[index + 2]; // V 
                buffer[i + 1] = data[index + 1]; // U
            }
            return buffer;
        }
    }
}
