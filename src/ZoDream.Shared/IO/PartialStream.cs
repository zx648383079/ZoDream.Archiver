using System;
using System.IO;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.IO
{
    /// <summary>
    /// 嵌套使用请手动更新源进度
    /// </summary>
    public class PartialStream: Stream, IReadOnlyStream, IStreamOrigin
    {
        /// <summary>
        /// 从当前位置获取剩余部分
        /// </summary>
        /// <param name="stream"></param>
        public PartialStream(Stream stream)
            : this (stream, stream.Position, stream.Length - stream.Position)
        {
            
        }

        public PartialStream(Stream stream, long byteLength)
            : this(stream, stream.Position, byteLength)
        { 
        }

        public PartialStream(Stream stream, long beginPosition, long byteLength)
        {
            var source = stream;
            if (stream is PartialStream ps)
            {
                beginPosition += ps._beginPosition;
                _syncStream = ps;
                source = ps.BaseStream;
            }
            if (source is EmptyStream)
            {
                beginPosition = 0;
                byteLength = 0;
            }
            _baseStream = source;
            _current = _beginPosition = beginPosition;
            _byteLength = byteLength;
        }
        private readonly Stream _baseStream;
        private readonly PartialStream? _syncStream;
        public Stream BaseStream => _baseStream;
        private readonly bool _leaveStreamOpen = true;
        private readonly long _beginPosition;
        private readonly long _byteLength;

        private long _current;

        private long EndPosition => _beginPosition + _byteLength;

        public override bool CanRead => _baseStream.CanRead;

        public override bool CanSeek => _baseStream.CanSeek;

        public override bool CanWrite => false;

        public override long Length => _byteLength;

        public override long Position {
            get => _current - _beginPosition;
            set {
                Seek(value, SeekOrigin.Begin);
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var len = (int)Math.Min(count, EndPosition - _current);
            if (len <= 0)
            {
                return 0;
            }
            _baseStream.SeekSkip(_current - _baseStream.Position);
            var res = _baseStream.Read(buffer, offset, len);
            SyncPosition(_current + res);
            return res;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            var min = _beginPosition;
            var max = _beginPosition + _byteLength;
            var pos = origin switch
            {
                SeekOrigin.Current => _current + offset,
                SeekOrigin.End => _beginPosition + _byteLength + offset,
                _ => _beginPosition + offset,
            };
            SyncPosition(Math.Min(Math.Max(pos, min), max));
            return _current - min;
        }

        private void SyncPosition(long current)
        {
            _current = current;
            _syncStream?.SyncPosition(current);
        }

        public override void Flush()
        {
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException(string.Empty);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException(string.Empty);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (_leaveStreamOpen == false)
            {
                _baseStream.Dispose();
            }
        }

    }
}
