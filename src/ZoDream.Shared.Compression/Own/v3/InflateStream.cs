using System;
using System.IO;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;

namespace ZoDream.Shared.Compression.Own.V3
{
    public class OwnInflateStream(Stream stream, 
        IOwnKey key, 
        long maxLength, 
        bool padding = true) : InflateStream(stream), IInflateStream
    {
        private long _position;
        private readonly int _chunkSize = 64;
        private readonly int _chunkCount = 8;
        private readonly byte[] _buffer = new byte[1024];
        private long _bufferPosition; // buffer 的开始位置
        private int _bufferSize; // buffer 中的有效尺寸

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException(string.Empty);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var res = 0;
            var chunkTotal = _chunkSize * _chunkCount;
            while (true)
            {
                if (_bufferSize == 0 || _position > _bufferPosition + _bufferSize)
                {
                    
                    _bufferPosition += _bufferSize;
                    _bufferSize = stream.Read(_buffer, 0, (int)Math.Min(_buffer.Length, maxLength - _bufferPosition));
                }
                var i = _position - _bufferPosition;
                var len = Math.Min(_bufferSize - i, count - res);
                var end = i + len;
                //for (; i < end; i++)
                //{

                //    buffer[offset + i] = Math.Floor(i / (double)chunkTotal) * chunkTotal + i ;
                //}
            }
            _position += res;
            return res;
        }
    }
}
