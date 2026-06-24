using System.IO;

namespace ZoDream.Shared.Compression.Own.V4
{
    public class OwnArchiveCompressor(IOwnKey key) : IOwnArchiveCompressor
    {
        public Stream CreateDeflator(Stream input, long length, bool padding)
        {
            return new V3.OwnDeflateStream(input, key, length, padding);
        }

        public Stream CreateInflator(Stream input, long length, bool padding)
        {
            return new V3.OwnInflateStream(input, key, length, padding);
        }
    }
}
