using System;

namespace ZoDream.Shared.Drawing
{
    public class RGB9e5F : SwapBufferDecoder
    {

        protected override void Decode(ReadOnlySpan<byte> input, int inputOffset, Span<byte> output, int outputOffset)
        {
            var n = BitConverter.ToInt32(input[inputOffset..]);
            var scale = n >> 27 & 0x1f;
            var scalef = Math.Pow(2, scale - 24);
            var b = n >> 18 & 0x1ff;
            var g = n >> 9 & 0x1ff;
            var r = n & 0x1ff;
            output[outputOffset + 2] = (byte)Math.Round(b * scalef * 255f);
            output[outputOffset + 1] = (byte)Math.Round(g * scalef * 255f);
            output[outputOffset] = (byte)Math.Round(r * scalef * 255f);
            output[outputOffset + 3] = byte.MaxValue;
        }

        protected override void Encode(ReadOnlySpan<byte> input, int inputOffset, Span<byte> output, int outputOffset)
        {
            throw new NotImplementedException();
        }
    }
}
