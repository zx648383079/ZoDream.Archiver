using System;
using System.Buffers.Binary;

namespace ZoDream.Shared.Drawing
{
    internal class VU1616(bool swapXY = false) : IBufferDecoder
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
                var X = (ushort)(BinaryPrimitives.ReadUInt16BigEndian(data[(index + 2)..]) + 0x7FFF);
                var Y = (ushort)(BinaryPrimitives.ReadUInt16BigEndian(data[index..]) + 0x7FFF);

                if (swapXY)
                {
                    output[index] = (byte)((X >> 8) & 0xFF);
                    output[index + 1] = (byte)(X & 0xFF);
                    output[index + 2] = (byte)((Y >> 8) & 0xFF);
                    output[index + 3] = (byte)(Y & 0xFF);
                }
                else
                {
                    output[index] = (byte)((Y >> 8) & 0xFF);
                    output[index + 1] = (byte)(Y & 0xFF);
                    output[index + 2] = (byte)((X >> 8) & 0xFF);
                    output[index + 3] = (byte)(X & 0xFF);
                }
            }
            return size * 4;
        }

    }
}
