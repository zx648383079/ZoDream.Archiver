using System;
using System.IO;

namespace ZoDream.Shared.IO
{
    public class PartialStream(Stream stream, long beginPostion, long byteLength): ReadOnlyStream(stream)
    {

        private long _current = beginPostion;

        private long EndPostion => beginPostion + byteLength;

        public override long Length => byteLength;

        public override long Position {
            get => _current - beginPostion;
            set {
                Seek(value + beginPostion, SeekOrigin.Begin);
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var len = (int)Math.Min(count, EndPostion - _current);
            if (len <= 0)
            {
                return 0;
            }
            stream.Seek(_current, SeekOrigin.Begin);
            return stream.Read(buffer, offset, len);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            var min = beginPostion;
            var max = beginPostion + byteLength;
            var pos = origin switch
            {
                SeekOrigin.Current => stream.Position + offset,
                SeekOrigin.End => beginPostion + byteLength + offset,
                _ => beginPostion + offset,
            };
            _current = Math.Min(Math.Max(pos, min), max);
            return _current - min;
        }

    }
}
