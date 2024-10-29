using System.IO;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;

namespace ZoDream.Shared.Compression.Own
{
    public class OwnDeflateStream(Stream stream, IOwnKey key, bool padding = true) : DeflateStream(stream), IDeflateStream
    {
        public override long Seek(long offset, SeekOrigin origin)
        {
            key.Seek(offset, origin);
            return base.Seek(offset, origin);
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            var res = stream.Read(buffer, offset, count);
            for (int i = 0; i < res; i++)
            {
                var code = key.ReadByte();
                buffer[offset + i] = (byte)OwnHelper.Clamp(
                    !padding ? (buffer[offset + i] + code) : (buffer[offset + i] - code)
                    , 256);
            }
            return res;
        }
    }
}
