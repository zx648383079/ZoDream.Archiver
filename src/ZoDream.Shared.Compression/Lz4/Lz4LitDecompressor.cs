namespace ZoDream.Shared.Compression.Lz4
{
    public class Lz4LitDecompressor(long unCompressedSize) : Lz4Decompressor(unCompressedSize)
    {
        protected override (int encCount, int litCount) GetLiteralToken(byte[] input, ref int inputPos) => ((input[inputPos] >> 4) & 0xf, (input[inputPos++] >> 0) & 0xf);
    }
}
