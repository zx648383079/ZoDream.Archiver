using System;
using System.Buffers.Binary;

namespace ZoDream.Shared.Drawing
{
    public class RGB565 : IBufferDecoder
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
                var temp = BinaryPrimitives.ReadUInt16BigEndian(data[offset..]);
                //var red = (byte)((temp >> 11) & 0x1f);
                //var green = (byte)((temp >> 5) & 0x3f);
                //var blue = (byte)(temp & 0x1f);

                //output[outputOffset + 0] = (byte)((red << 3) | (red >> 2));
                //output[outputOffset + 1] = (byte)((green << 2) | (green >> 4));
                //output[outputOffset + 2] = (byte)((blue << 3) | (blue >> 2));
                //output[outputOffset + 3] = 0xFF;
                output[outputOffset + 0] = (byte)(temp & 0xF800);
                output[outputOffset + 1] = (byte)(temp & 0x07E0);
                output[outputOffset + 2] = (byte)(temp & 0x001F);
                output[outputOffset + 3] = byte.MaxValue;
            }
            return size * 4;
        }
    }
}
