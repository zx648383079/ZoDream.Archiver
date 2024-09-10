using System;
using System.IO;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.IO
{
    public class PartialStream: Stream, IReadOnlyStream
    {
        public PartialStream(Stream stream, long byteLength)
            : this(stream, stream.Position, byteLength)
        { 
        }

        public PartialStream(Stream stream, long beginPostion, long byteLength)
        {
            _byteLength = byteLength;
            if (stream is not PartialStream ps)
            {
                BaseStream = stream;
                _current = _beginPostion = beginPostion;
                return;
            }
            BaseStream = ps.BaseStream;
            _current = _beginPostion = beginPostion + ps._beginPostion;
        }

        private readonly Stream BaseStream;

        private readonly long _beginPostion;
        private readonly long _byteLength;

        private long _current;

        private long EndPostion => _beginPostion + _byteLength;

        public override bool CanRead => BaseStream.CanRead;

        public override bool CanSeek => BaseStream.CanSeek;

        public override bool CanWrite => false;

        public override long Length => _byteLength;

        public override long Position {
            get => _current - _beginPostion;
            set {
                Seek(value + _beginPostion, SeekOrigin.Begin);
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var len = (int)Math.Min(count, EndPostion - _current);
            if (len <= 0)
            {
                return 0;
            }
            BaseStream.Seek(_current, SeekOrigin.Begin);
            var res = BaseStream.Read(buffer, offset, len);
            _current += res;
            return res;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            var min = _beginPostion;
            var max = _beginPostion + _byteLength;
            var pos = origin switch
            {
                SeekOrigin.Current => BaseStream.Position + offset,
                SeekOrigin.End => _beginPostion + _byteLength + offset,
                _ => _beginPostion + offset,
            };
            _current = Math.Min(Math.Max(pos, min), max);
            return _current - min;
        }

        public override void Flush()
        {
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}
