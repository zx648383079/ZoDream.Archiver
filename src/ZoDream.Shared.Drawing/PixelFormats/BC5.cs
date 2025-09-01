using System;

namespace ZoDream.Shared.Drawing
{
    public class BC5 : BlockBufferDecoder
    {
        protected override int BlockSize => 16;
        /// <summary>
        /// RG
        /// </summary>
        protected override int BlockPixelSize => 2;
        protected override void DecodeBlock(ReadOnlySpan<byte> data, Span<byte> output)
        {
            int destinationPitch = 4 * 2;
            BC3.SmoothAlphaBlock(data, output, destinationPitch, 2);
            BC3.SmoothAlphaBlock(data[8..], output[1..], destinationPitch, 2);
        }

    }
}
