using System.IO;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;

namespace ZoDream.Shared.Compression.Own.V3
{
    public class OwnInflateStream(Stream stream, 
        IOwnKey key, long maxLength, 
        bool padding = true) : InflateStream(stream), IInflateStream
    {
        private long _position;

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException(string.Empty);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var res = stream.Read(buffer, offset, count);
            return res;
        }
    }
}
