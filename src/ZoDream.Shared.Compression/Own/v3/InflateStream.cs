using System;
using System.Buffers;
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

        public override bool CanSeek => false;

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException(string.Empty);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var res = 0;
            while (res < count)
            {
                if (_position >= maxLength)
                {
                    break;
                }
                if (_bufferSize == 0 || _position >= _bufferPosition + _bufferSize)
                {
                    TryRead();
                }
                if (_bufferSize == 0)
                {
                    break;
                }
                var i = _position - _bufferPosition;
                var len = (int)Math.Min(_bufferSize - i, count - res);
                if (len <= 0)
                {
                    break;
                }
                Array.Copy(_buffer, i, buffer, offset + res, len);
                res += len;
                _position += len;
            }
            return res;
        }

        private void TryRead()
        {
            _bufferPosition += _bufferSize;
            var length = (int)Math.Min(_buffer.Length, maxLength - _bufferPosition);
            var buffer = ArrayPool<byte>.Shared.Rent(length);
            _bufferSize = stream.Read(buffer, 0, length);
            if (_bufferSize == 0)
            {
                ArrayPool<byte>.Shared.Return(buffer);
                return;
            }
            var chunkTotal = _chunkSize * _chunkCount;
            var maxChunkOffset = _bufferSize - (_bufferSize % chunkTotal);
            for (var i = 0; i < _bufferSize; i++)
            {
                var code = key.ReadByte();
                var j = i;
                if (i < maxChunkOffset)
                {
                    var rate = i % chunkTotal;
                    var max = i - rate;
                    j = max + rate % _chunkCount * _chunkSize + rate / _chunkCount;
                }
                _buffer[i] = (byte)OwnHelper.Clamp(
                    padding ? (buffer[j] + code) : (buffer[j] - code)
                    , 256);
            }
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}
