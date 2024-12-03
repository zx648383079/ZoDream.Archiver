using System.IO;

namespace ZoDream.Shared.Interfaces
{
    public interface ICompressor
    {
        public byte[] Compress(byte[] input);

        public void Compress(Stream input, Stream output);
    }
    public interface IDecompressor
    {
        public byte[] Decompress(byte[] input);

        public void Decompress(Stream input, Stream output);
    }
}
