using System;

namespace ZoDream.Shared.Drawing
{
    /// <summary>
    /// @see https://github.com/AssetRipper/TextureDecoder
    /// </summary>
    public class BC4 : BlockBufferDecoder
    {
        protected override int BlockSize => 8;
        /// <summary>
        /// R
        /// </summary>
        protected override int BlockPixelSize => 1;
        protected override void DecodeBlock(ReadOnlySpan<byte> data, Span<byte> output)
        {
            var destinationPitch = 4 * 1;
            BC3.SmoothAlphaBlock(data, output, destinationPitch, 1);
            
        }

    }
}
