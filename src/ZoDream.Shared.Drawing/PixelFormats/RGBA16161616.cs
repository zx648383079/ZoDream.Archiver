using System;
using System.Buffers.Binary;

namespace ZoDream.Shared.Drawing
{
    public class RGBA16161616 : IBufferDecoder
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
                var offset = i * 6;
                output[index] = ColorConverter.From16BitTo8Bit(
                    BinaryPrimitives.ReadUInt16BigEndian(data[offset..])
                );
                output[index + 1] = ColorConverter.From16BitTo8Bit(
                   BinaryPrimitives.ReadUInt16BigEndian(data[(offset + 2)..])
                );
                output[index + 2] = ColorConverter.From16BitTo8Bit(
                    BinaryPrimitives.ReadUInt16BigEndian(data[(offset + 4)..])
                );
                output[index + 3] = ColorConverter.From16BitTo8Bit(
                    BinaryPrimitives.ReadUInt16BigEndian(data[(offset + 6)..])
                );
            }
            return size * 4;
        }
    }
}
