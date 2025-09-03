using System;

namespace ZoDream.Shared.Drawing
{
    /// <summary>
    /// @see https://github.com/AssetRipper/TextureDecoder
    /// </summary>
    internal class EACRGSigned : BlockBufferDecoder
    {
        protected override int BlockSize => 16;

        protected override void DecodeBlock(ReadOnlySpan<byte> data, Span<byte> output)
        {
            EACRSigned.DecodeEacSignedBlock(data[..8], output, 2);
            EACRSigned.DecodeEacSignedBlock(data.Slice(8, 8), output, 1);
        }
    }
}
