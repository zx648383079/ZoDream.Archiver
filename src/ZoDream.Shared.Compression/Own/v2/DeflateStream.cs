using System.IO;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;

namespace ZoDream.Shared.Compression.Own.V2
{
    public class OwnDeflateStream(Stream stream, IOwnKey key, byte[] iv, bool padding = true) : DeflateStream(stream), IDeflateStream
    {
        private long _position;
        public override bool CanSeek => false;
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException(string.Empty);
            //key.Seek(offset, origin);
            //return base.Seek(offset, origin);
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            var res = stream.Read(buffer, offset, count);
            for (int i = 0; i < res; i++)
            {
                var code = key.ReadByte();
                var pos = _position % iv.Length;
                var index = offset + i;
                var old = buffer[index];
                var b = buffer[index] ^ iv[pos];
                buffer[index] = (byte)OwnHelper.Clamp(
                    !padding ? (b + code) : (b - code)
                    , 256);
                iv[pos] = old;
                _position++;
            }
            return res;
        }
    }
}
