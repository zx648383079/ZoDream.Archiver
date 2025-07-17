using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.IO
{
    /// <summary>
    /// 乱序流重排
    /// </summary>
    public class OrderedStream(Stream input, Tuple<long, long>[] partItems) : Stream, IReadOnlyStream, IStreamOrigin
    {

        private long _current = 0;

        public Stream BaseStream => input;

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override long Length => partItems.Select(i => i.Item2).Sum();

        public override long Position 
        { 
            get => _current; 
            set => Seek(value, SeekOrigin.Begin); 
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var len = 0;
            var begin = 0;
            foreach (var item in partItems)
            {
                var end = begin + item.Item2;
                if (end < _current || begin > _current)
                {
                    continue;
                }
                var pos = _current - begin;
                input.Seek(pos + item.Item1, SeekOrigin.Begin);
                var c = input.Read(buffer, offset, Math.Min((int)(item.Item2 - pos), count));
                _current += c;
                len += c;
                offset += c;
                count -= c;
                if (count <= 0)
                {
                    break;
                }
            }
            return len;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            var pos = origin switch
            {
                SeekOrigin.Current => _current + offset,
                SeekOrigin.End => Length + offset,
                _ => offset,
            };
            if (pos < 0)
            {
                pos = 0;
            } else if (pos >= Length)
            {
                pos = Length - 1;
            }
            return _current = pos;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }
        public override void Flush()
        {
            throw new NotImplementedException();
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }


        public static Stream Create(Stream input, [MinLength(1)] params long[] splitPositions)
        {
            if (splitPositions.Length == 0 || (splitPositions.Length == 1 && splitPositions[0] == 0))
            {
                return input;
            }
            var total = input.Length;
            if (splitPositions.Length == 1)
            {
                return new OrderedStream(input, [
                    new(splitPositions[0], total - splitPositions[0]),
                    new(0, splitPositions[0])
                ]);
            }
            var sortItems = splitPositions.Order().ToArray();
            return new OrderedStream(input,
                [.. splitPositions.Select(pos => {
                    var i = Array.IndexOf(sortItems, pos);
                    long len = i == sortItems.Length - 1 ? (total - pos) : (sortItems[i + 1] - pos);
                    return new Tuple<long, long>(pos, len);
                })]
            );
        }
    }
}
