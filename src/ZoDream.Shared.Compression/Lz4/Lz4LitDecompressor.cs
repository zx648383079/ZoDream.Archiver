using System.IO;

namespace ZoDream.Shared.Compression.Lz4
{
    public class Lz4LitDecompressor(long unCompressedSize) : Lz4Decompressor(unCompressedSize)
    {
        protected override (int encCount, int litCount) GetLiteralToken(Stream input)
        {
            var b = (byte)input.ReadByte();
            return ((b >> 4) & 0xf, (b >> 0) & 0xf);
        }
    }
}
