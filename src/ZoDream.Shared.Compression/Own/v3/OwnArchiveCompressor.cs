using System.IO;

namespace ZoDream.Shared.Compression.Own.V3
{
    public class OwnArchiveCompressor(IOwnKey key) : IOwnArchiveCompressor
    {
        public Stream CreateDeflator(Stream input, long length, bool padding)
        {
            return new OwnDeflateStream(input, key, length, padding);
        }

        public Stream CreateInflator(Stream input, long length, bool padding)
        {
            return new OwnInflateStream(input, key, length, padding);
        }
    }
}
