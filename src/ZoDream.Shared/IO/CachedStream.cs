using System;
using System.IO;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.IO
{
    /// <summary>
    /// 有缓存的只读流
    /// </summary>
    /// <param name="cacheSize"></param>
    public class CachedStream(Stream input, int cacheSize = 1024) : Stream, IReadOnlyStream
    {

        private readonly byte[] _buffer = new byte[cacheSize];
        private long _bufferBegin;
        private long _bufferSize;
        private long _bufferOffset;

        /// <summary>
        /// 返回正常流
        /// </summary>
        public Stream BaseStream 
        { 
           get 
           {
                // 应用位置，并返回流
                input.Seek(Position, SeekOrigin.Begin);
                _bufferBegin = Position;
                _bufferSize = 0;
                return input;
           } 
        }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override long Length => input.Length;

        public override long Position 
        { 
            get => _bufferBegin + _bufferOffset; 
            set => Seek(value, SeekOrigin.Begin);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var res = 0;
            while (res < count)
            {
                if (_bufferSize == 0 || _bufferOffset >= _bufferSize)
                {
                    _bufferBegin = input.Position;
                    _bufferSize = input.Read(_buffer, 0, _buffer.Length);
                    _bufferOffset = 0;
                }
                if (_bufferSize == 0)
                {
                    break;
                }
                var i = _bufferOffset;
                var len = (int)Math.Min(_bufferSize - i, count - res);
                if (len <= 0)
                {
                    break;
                }
                Array.Copy(_buffer, i, buffer, offset + res, len);
                res += len;
                _bufferOffset += len;
            }
            return res;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            var pos = origin switch
            {
                SeekOrigin.Current => Position + offset,
                SeekOrigin.End => Length + offset,
                _ => offset
            };
            offset = pos - _bufferBegin;
            if (offset >= 0 && offset < _bufferSize)
            {
                _bufferOffset = offset;
                return Position;
            }
            var res = input.Seek(pos, SeekOrigin.Begin);
            _bufferBegin = res;
            _bufferSize = 0;
            _bufferOffset = 0;
            return res;
        }

        public override void Flush()
        {
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            input.Dispose();
        }
    }
}
