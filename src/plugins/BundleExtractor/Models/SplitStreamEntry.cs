using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.Models
{
    public class SplitStreamEntry(long length, long compressedLength)
    {
        public SplitStreamEntry(long length, long compressedLength, 
            CompressionType compression)
            : this(length, compressedLength)
        {
            CompressionType = compression;
        }
        public long Length { get; private set; } = length;

        public long CompressedLength { get; private set; } = compressedLength;

        public virtual CompressionType CompressionType { get; } = CompressionType.None;
    }

    public class MergeSplitStreamEntry(SplitStreamEntry entry)
    {
        public SplitStreamEntry Source { get; set; } = entry;
        public long Offset { get; set; }

        public long CompressedPosition { get; set; }

        public long EffectiveLength { get; set; }
    }

    public class SplitStreamCollection: List<SplitStreamEntry> 
    {

        private Stream? _cacheSource;
        private SplitStreamEntry? _cacheEntry;

        public SplitStreamCollection()
        {
            
        }
        public SplitStreamCollection(IEnumerable<SplitStreamEntry> collection) : base(collection)
        {

        }

        public IEnumerable<MergeSplitStreamEntry> GetRange(long offset, long length)
        {
            var end = offset + length;
            var pos = 0L;
            var p = 0L;
            foreach (var item in this)
            {
                var next = pos + item.Length;
                var maxBegin = Math.Max(pos, offset);
                if (maxBegin < Math.Min(next, end))
                {
                    yield return new(item)
                    {
                        CompressedPosition = p,
                        Offset = Math.Max(0, offset - pos),
                        EffectiveLength = Math.Min(item.Length, end - maxBegin)
                    };
                }
                if (next > end)
                {
                    break;
                }
                pos = next;
                p += item.CompressedLength;
            }
        }

        private Stream CreateEntryStream(Stream input, long inputBasePos, MergeSplitStreamEntry entry)
        {
            var ms = CreateEntryStream(input, inputBasePos + entry.CompressedPosition, entry.Source);
            return new PartialStream(ms, entry.Offset, entry.EffectiveLength);
        }

        private Stream CreateEntryStream(Stream input, long inputBasePos, SplitStreamEntry entry)
        {
            if (_cacheEntry == entry && _cacheSource is not null)
            {
                return _cacheSource;
            }
            var stream = BundleCodec.Decode(new PartialStream(input,
                inputBasePos,
                    entry.CompressedLength), entry.CompressionType,
                    entry.Length);
            try
            {
                _ = stream.Position;
            }
            catch (Exception)
            {
                var ms = new MemoryStream();
                stream.CopyTo(ms);
                stream.Dispose();
                stream = ms;
            }
            _cacheEntry = entry;
            _cacheSource = stream;
            return stream;
        }

        public Stream Create(Stream input, long offset, long length)
        {
            var basePosition = input.Position;
            var items = GetRange(offset, length).Select(item => CreateEntryStream(input, basePosition, item)).ToArray();
            if (items.Length == 1)
            {
                return items[0];
            }
            return new MultipartFileStream(items);
        }
    }
}
