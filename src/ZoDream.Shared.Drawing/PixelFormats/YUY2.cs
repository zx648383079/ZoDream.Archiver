using System;

namespace ZoDream.Shared.Drawing
{
    public class YUY2 : IBufferDecoder, IBufferEncoder
    {
        public byte[] Decode(ReadOnlySpan<byte> data, int width, int height)
        {
            var buffer = new byte[width * height * 4];
            Decode(data, width, height, buffer);
            return buffer;
        }

        public int Decode(ReadOnlySpan<byte> data, int width, int height, Span<byte> output)
        {
            var p = 0;
            var o = 0;
            var halfWidth = width / 2;
            for (var j = 0; j < height; j++)
            {
                for (var i = 0; i < halfWidth; ++i)
                {
                    var y0 = data[p++];
                    var u0 = data[p++];
                    var y1 = data[p++];
                    var v0 = data[p++];
                    YV12.ToRGBA(y0, u0, v0, output[o..]);
                    o += 4;
                    YV12.ToRGBA(y1, u0, v0, output[o..]);
                    o += 4;
                }
            }
            return width * height * 4;
        }

        public byte[] Encode(ReadOnlySpan<byte> data, int width, int height)
        {
            var buffer = new byte[width * height * 2];
            var ptrY = 0;
            for (var i = 0; i < height; i++)
            {
                var begin = i * width;
                for (var j = 0; j < width; j++)
                {
                    var offset = (begin + j) * 4;
                    var (y, u, v) = YV12.ToYUV(data[offset..]);
                    if (j % 2 == 0)
                    {
                        buffer[ptrY++] = y;
                        buffer[ptrY++] = u;
                    } else
                    {
                        buffer[ptrY++] = y;
                        buffer[ptrY++] = v;
                    }
                }
            }
            return buffer;
        }
    }
}
