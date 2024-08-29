using System;

namespace ZoDream.Shared.Interfaces
{
    public interface IReadOnlyEntry
    {
        public string Name { get; }

        public long Length { get; }

        public long CompressedLength { get; }

        public bool IsEncrypted { get; }

        public DateTime? CreatedTime { get; }
    }
}
