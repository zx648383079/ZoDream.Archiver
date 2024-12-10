using System.IO;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;

namespace ZoDream.Shared.Compression.Own.V2
{
    public class OwnInflateStream(Stream stream, IOwnKey key, byte[] iv, bool padding = true) : InflateStream(stream), IInflateStream
    {
        private long _position;
        public override bool CanSeek => false;
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException(string.Empty);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var res = stream.Read(buffer, offset, count);
            for (int i = 0; i < res; i++)
            {
                var code = key.ReadByte();
                var pos = _position % iv.Length;
                var index = offset + i;
                iv[pos] = buffer[index] = (byte)((byte)OwnHelper.Clamp(
                    padding ? (buffer[index] + code) : (buffer[index] - code)
                    , 256) ^ iv[pos]);
                _position ++;
            }
            return res;
        }
    }
}
