using System.IO;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;

namespace ZoDream.Shared.Compression.Own.V3
{
    public class OwnDeflateStream(
        Stream stream, IOwnKey key,
        long maxLength,
        bool padding = true) : DeflateStream(stream), IDeflateStream
    {
        private long _position;

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException(string.Empty);
            //key.Seek(offset, origin);
            //return base.Seek(offset, origin);
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            var res = stream.Read(buffer, offset, count);
            return res;
        }
    }
}
