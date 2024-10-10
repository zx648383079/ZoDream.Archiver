using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.IO
{
    /// <summary>
    /// 分卷流读取
    /// </summary>
    /// <param name="items"></param>
    public class MultipartFileStream(IEnumerable<Stream> items) : Stream, IReadOnlyStream
    {
        private readonly Stream[] _items = items.ToArray();
        private int _index;
        /// <summary>
        /// 执行销毁时不销毁的流
        /// </summary>
        private int _leaveOpenStream = -1;
        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override long Length => _items.Sum(t => t.Length);

        public override long Position {
            get {
                var pos = 0L;
                for (var i = 0; i < _index; i++)
                {
                    pos += _items[i].Length;
                }
                return pos + _items[_index].Position;
            }
            set {
                Seek(value, SeekOrigin.Begin);
            }
        }

        public override void Flush()
        {
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
                _index = _items.Length - 1;
                _items[_index].Seek(0, SeekOrigin.End);
                return total;
            }
            var res = pos;
            for (var i = 0; i < _items.Length; i++)
            {
                if (pos < _items[i].Length)
                {
                    _index = i;
                    _items[i].Seek(pos, SeekOrigin.Begin);
                    break;
                }
                pos -= _items[i].Length;
            }
            return res;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var len = 0;
            while (true)
            {
                var res = _items[_index].Read(buffer, offset + len, count - len);
                len += res;
                if (len >= count)
                {
                    break;
                }
                if (_index >= _items.Length - 1)
                {
                    break;
                }
                _index++;
                _items[_index].Seek(0, SeekOrigin.Begin);
            }
            return len;
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
            var i = 0;
            foreach (var item in items)
            {
                if (_leaveOpenStream == i)
                {
                    continue;
                }
                item.Dispose();
                i++;
            }
        }

        public static MultipartFileStream Open(IEnumerable<string> fileItems)
        {
            return Open(fileItems, string.Empty, null);
        }

        public static MultipartFileStream Open(
            IEnumerable<string> fileItems, string openFile, Stream? openStream)
        {
            if (!fileItems.Any())
            {
                throw new ArgumentNullException(nameof(fileItems));
            }
            var items = new Stream[fileItems.Count()];
            try
            {
                var i = 0;
                var leaveOpenStream = -1;
                foreach (var item in fileItems)
                {
                    if (openStream is not null && item == openFile)
                    {
                        leaveOpenStream = i;
                        items[i++] = openStream;
                        continue;
                    }
                    items[i ++] = File.OpenRead(item);
                }
                return new MultipartFileStream(items)
                {
                    _leaveOpenStream = leaveOpenStream,
                };
            }
            catch
            {
                foreach (var item in items)
                {
                    if (item is null || item == openStream)
                    {
                        break;
                    }
                    item.Dispose();
                }
                throw;
            }
        }
    }
}
