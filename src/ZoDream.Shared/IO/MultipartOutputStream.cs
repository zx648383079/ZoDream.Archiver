using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.IO
{
    /// <summary>
    /// 分卷流写入
    /// </summary>
    public class MultipartOutputStream : Stream, IWriteOnlyStream
    {
        public MultipartOutputStream(Stream stream, string fileName, long maxLength)
            : this(fileName, maxLength)
        {
            _firstNeedRename = true;
            _items.Add(stream);
        }

        public MultipartOutputStream(string fileName, long maxLength)
            : this(Path.GetDirectoryName(fileName)!, Path.GetFileName(fileName), maxLength)
        {
            
        }

        public MultipartOutputStream(string folder, string name, long maxLength)
        {
            _folder = folder;
            _namePrefix = name;
            _maxLength = maxLength;
        }

        /// <summary>
        /// 表示第一个文件的命名方式不是多文件格式
        /// </summary>
        private bool _firstNeedRename = true;
        private readonly string _folder;
        private readonly string _namePrefix;
        private readonly long _maxLength;
        private readonly List<Stream> _items = [];
        private int _index;

        public override bool CanRead => false;

        public override bool CanSeek => true;

        public override bool CanWrite => true;

        public override long Length => _items.Sum(t => t.Length);

        public override long Position {
            get {
                return _index * _maxLength + GetStream(_index).Position;
            }
            set {
                Seek(value, SeekOrigin.Begin);
            }
        }

        public override void Flush()
        {
            foreach (var item in _items)
            {
                item.Flush();
            }
        }


        public override long Seek(long offset, SeekOrigin origin)
        {
            var total = Length;
            var pos = origin switch
            {
                SeekOrigin.Current => Position + offset,
                SeekOrigin.End => total + offset,
                _ => offset
            };
            pos = Math.Max(pos, 0);
            if (pos >= total)
            {
                _index = _items.Count - 1;
                _items[_index].Seek(0, SeekOrigin.End);
                return total;
            }
            var count = GetFileCount(pos);
            var res = GetStream(count - 1).Seek(pos % _maxLength, SeekOrigin.Begin);
            return (count - 1) * _maxLength + res;
        }


        public override void SetLength(long value)
        {
            var count = GetFileCount(value);
            var offset = 0L;
            for (var i = 0; i < count; i++)
            {
                GetStream(i).SetLength(Math.Min(_maxLength, value - offset));
                offset += _maxLength;
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            while (true)
            {
                var fs = GetStream(_index);
                var len = (int)Math.Min(_maxLength - fs.Position, count);
                fs.Write(buffer, offset, len);
                offset += len;
                count -= len;
                if (count <= 0)
                {
                    break;
                }
                _index++;
                GetStream(_index).Seek(0, SeekOrigin.Begin);
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException(string.Empty);
        }

        private int GetFileCount(long length)
        {
            return (int)Math.Ceiling((double)length / _maxLength);
        }

        private Stream GetStream(int index)
        {
            if (_items.Count > index)
            {
                return _items[index];
            }
            for (var i = _items.Count; i <= index; i++)
            {
                var fullPath = i < 1 ? Path.Combine(_folder, _namePrefix) : GetFullPath(index);
                _items.Add(File.Create(fullPath));
            }
            return _items[index];
        }

        private string GetFullPath(int index)
        {
            return Path.Combine(_folder, _namePrefix + (index + 1).ToString().PadLeft(3, '0'));
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            foreach (var item in _items)
            {
                item.Dispose();
            }
            if (_firstNeedRename && _items.Count > 1)
            {
                File.Move(Path.Combine(_folder, _namePrefix), GetFullPath(0));
            }
        }

    }
}
