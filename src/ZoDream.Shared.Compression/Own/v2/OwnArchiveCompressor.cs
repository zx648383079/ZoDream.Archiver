using System.IO;

namespace ZoDream.Shared.Compression.Own.V2
{
    public class OwnArchiveCompressor(IOwnKey key) : IOwnArchiveCompressor
    {
        public Stream CreateDeflator(Stream input, long length, bool padding)
        {
            return new OwnDeflateStream(input, key, new byte[length >= 64 ? 64 : 16], padding);
        }

        public Stream CreateInflator(Stream input, long length, bool padding)
        {
            return new OwnInflateStream(input, key, new byte[length >= 64 ? 64 : 16], padding);
        }
    }
}
