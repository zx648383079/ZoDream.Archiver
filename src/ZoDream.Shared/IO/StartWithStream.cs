using System;
using System.IO;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.IO
{
    /// <summary>
    /// 在流的前面预设一些字节，例如：把删除的文件头加上
    /// </summary>
    /// <param name="data"></param>
    /// <param name="next"></param>
    public class StartWithStream(byte[] data, Stream next) : Stream, IReadOnlyStream
    {

        private long _position = 0;

        public override bool CanRead => next.CanRead;

        public override bool CanSeek => next.CanSeek;

        public override bool CanWrite => false;

        public override long Length => next.Length + data.Length;

        public override long Position { 
            get => next.Position > 0 ? next.Position + data.Length : _position; 
            set => Seek(value, SeekOrigin.Begin); 
        }

       

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (next.Position > 0)
            {
                _position = Position;
            }
            var res = 0;
            var len = (int)(data.Length - _position);
            if (len > 0)
            {
                Array.Copy(data, _position, buffer, offset, len);
                res += len;
                _position += len;
            }
            if (count > res)
            {
                len = next.Read(buffer, offset + res, count - res);
                _position += len;
                res += len;
            }
            return res;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            _position = origin switch
            {
                SeekOrigin.Current => Position + offset,
                SeekOrigin.End => Length + offset,
                _ => offset
            };
            if (_position < data.Length)
            {
                next.Seek(0, SeekOrigin.Begin);
                return _position;
            }
            _position = next.Seek(Math.Max(0, _position - data.Length), SeekOrigin.Begin) + data.Length;
            return _position;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override void Flush()
        {
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                next.Dispose();
            }
        }
    }
}
