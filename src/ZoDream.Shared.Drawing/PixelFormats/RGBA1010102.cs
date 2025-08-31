using System;

namespace ZoDream.Shared.Drawing
{
    internal class RGBA1010102 : IBufferDecoder
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
                var packed = BitConverter.ToUInt32(data[index..]);
                output[index] = (byte)(((packed >> 0) & 0x03FF) / 1023F);
                output[index + 1] = (byte)(((packed >> 10) & 0x03FF) / 1023F);
                output[index + 2] = (byte)(((packed >> 20) & 0x03FF) / 1023F);
                output[index + 3] = (byte)(((packed >> 30) & 0x03) / 3);
            }
            return size * 4;
        }
    }
}
