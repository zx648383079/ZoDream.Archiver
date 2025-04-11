using System;
using System.Buffers;
using System.IO;

namespace ZoDream.Shared.IO
{
    /// <summary>
    /// 使用 ArrayPool<byte>.Shared 租借的内存，用于频繁创建销毁的临时数据，需要使用 using
    /// </summary>
    public class ArrayMemoryStream : Stream
    {
        public ArrayMemoryStream(int capacity)
        {
            _capacity = capacity;
            _buffer = ArrayPool<byte>.Shared.Rent(capacity);
        }

        private readonly int _capacity;
        private int _position;
        private readonly byte[] _buffer;
        private bool _isOpen = true;

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => true;

        public override long Length => _capacity;

        public override long Position { 
            get => _position; 
            set => Seek(value, SeekOrigin.Begin); 
        }

        private void EnsureNotClosed()
        {
            if (_isOpen)
            {
                return;
            }
            throw new ObjectDisposedException(null);
        }
        /// <summary>
        /// 获取原始数据
        /// </summary>
        /// <returns></returns>
        public virtual byte[] GetBuffer()
        {
            EnsureNotClosed();
            return _buffer;
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            EnsureNotClosed();
            var num = _capacity - _position;
            if (num > count)
            {
                num = count;
            }
            if (num <= 0)
            {
                return 0;
            }
            if (num <= 8)
            {
                int num2 = num;
                while (--num2 >= 0)
                {
                    buffer[offset + num2] = _buffer[_position + num2];
                }
            }
            else
            {
                Buffer.BlockCopy(_buffer, _position, buffer, offset, num);
            }
            _position += num;
            return num;
        }

        public override int Read(Span<byte> buffer)
        {
            EnsureNotClosed();
            int num = Math.Min(_capacity - _position, buffer.Length);
            if (num <= 0)
            {
                return 0;
            }
            new Span<byte>(_buffer, _position, num).CopyTo(buffer);
            _position += num;
            return num;
        }

        public override int ReadByte()
        {
            EnsureNotClosed();
            if (_position >= _capacity)
            {
                return -1;
            }
            return _buffer[_position++];
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            EnsureNotClosed();
            var loc = origin switch
            {
                SeekOrigin.Current => _position,
                SeekOrigin.End => _capacity,
                _ => 0,
            };
            var num = loc + (int)offset;
            if (num < 0)
            {
                num = 0;
            }
            if (num > _capacity)
            {
                num = _capacity;
            }
            _position = num;
            return _position;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            EnsureNotClosed();
            count = Math.Min(_capacity - _position, count);
            if (count < 0)
            {
                return;
            }
            var num = _position + count;
            if (count <= 8 && buffer != _buffer)
            {
                int num2 = count;
                while (--num2 >= 0)
                {
                    _buffer[_position + num2] = buffer[offset + num2];
                }
            }
            else
            {
                Buffer.BlockCopy(buffer, offset, _buffer, _position, count);
            }
            _position = num;
        }


        public override void WriteByte(byte value)
        {
            EnsureNotClosed();
            if (_position >= _capacity)
            {
                return;
            }
            _buffer[_position++] = value;
        }

        public override void CopyTo(Stream destination, int bufferSize)
        {
            EnsureNotClosed();
            ValidateCopyToArguments(destination, bufferSize);
            int position = _position;
            int num = _capacity - position;
            if (num > 0)
            {
                destination.Write(_buffer, position, num);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing && _isOpen)
            {
                _isOpen = false;
                ArrayPool<byte>.Shared.Return(_buffer);
            }
        }
    }
}
