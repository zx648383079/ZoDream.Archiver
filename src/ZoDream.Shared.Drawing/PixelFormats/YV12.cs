using System;

namespace ZoDream.Shared.Drawing
{
    public class YV12 : IBufferDecoder, IBufferEncoder
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
                // TODO
            }
            return size * 4;
        }

        public byte[] Encode(ReadOnlySpan<byte> data, int width, int height)
        {
            var buffer = new byte[width * height * 3 / 2];
            var ptrY = 0;
            var ptrV = ptrY + width * height;
            var ptrU = ptrV + width * height / 4;
            for (var i = 0; i < height; i++)
            {
                var begin = i * width;
                for (var j = 0; j < width; j++)
                {
                    var offset = (begin + j) * 4;
                    var r = data[offset];
                    var g = data[offset + 1];
                    var b = data[offset + 2];
                    var y = ((66 * r + 129 * g + 25 * b) >> 8) + 16;
                    var u = ((-38 * r - 74 * g + 112 * b) >> 8) + 128;
                    var v = ((112 * r - 94 * g - 18 * b) >> 8) + 128;
                    buffer[ptrY ++] = (byte)y;
                    if (j % 2 == 0)
                    {
                        if (i % 2 == 0)
                        {
                            buffer[ptrV ++] = (byte)v;
                        } else
                        {
                            buffer[ptrU ++] = (byte)u;
                        }
                    }
                }
            }
            return buffer;
        }
    }
}
