using System.IO;

namespace ZoDream.Shared.Compression.Own
{
    public class OwnArchiveCompressor(IOwnKey key) : IOwnArchiveCompressor
    {
        public Stream CreateDeflator(Stream input, long length, bool padding)
        {
            return new OwnDeflateStream(input, key, padding);
        }

        public Stream CreateInflator(Stream input, long length, bool padding)
        {
            return new OwnInflateStream(input, key, padding);
        }
    }
}
