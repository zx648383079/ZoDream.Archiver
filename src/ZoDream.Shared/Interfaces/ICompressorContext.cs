using System.IO;

namespace ZoDream.Shared.Interfaces
{
    public interface ICompressor
    {
        public byte[] Compress(byte[] input);
        public byte[] Compress(byte[] input, int inputLength);

        public void Compress(Stream input, Stream output);
    }
    public interface IDecompressor
    {
        public byte[] Decompress(byte[] input);
        public byte[] Decompress(byte[] input, int inputLength);

        public void Decompress(Stream input, Stream output);
    }
}
