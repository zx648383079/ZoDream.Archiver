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
            var vShift = size / 4;
            for (int y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var shift = y / 2 * (width / 2) + (x / 2);
                    var y0 = data[y * width + x];
                    var u = data[size + shift];
                    var v = data[size + shift + vShift];
                    var index = (y * width + x) * 4;
                    ToRGBA(y0, u, v, output[index..]);
                }
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
                    var (y, u, v) = ToYUV(data[offset..]);
                    buffer[ptrY ++] = y;
                    if (j % 2 == 0)
                    {
                        if (i % 2 == 0)
                        {
                            buffer[ptrV ++] = v;
                        } else
                        {
                            buffer[ptrU ++] = u;
                        }
                    }
                }
            }
            return buffer;
        }

        internal static (byte y, byte u, byte v) ToYUV(ReadOnlySpan<byte> data)
        {
            var r = data[0];
            var g = data[1];
            var b = data[2];
            var y = ((66 * r + 129 * g + 25 * b) >> 8) + 16;
            var u = ((-38 * r - 74 * g + 112 * b) >> 8) + 128;
            var v = ((112 * r - 94 * g - 18 * b) >> 8) + 128;
            return ((byte)y, (byte)u, (byte)v);
        }

        internal static void ToRGBA(byte y, byte u, byte v, Span<byte> output)
        {
            var c = y - 16;
            var d = u - 128;
            var e = v - 128;
            output[0] = (byte)Math.Clamp((298 * c + 516 * d + 128) >> 8, byte.MinValue, byte.MaxValue);
            output[1] = (byte)Math.Clamp((298 * c - 100 * d - 208 * e + 128) >> 8, byte.MinValue, byte.MaxValue);
            output[2] = (byte)Math.Clamp((298 * c + 409 * e + 128) >> 8, byte.MinValue, byte.MaxValue);
            output[3] = byte.MaxValue;
        }
    }
}
