using System;

namespace ZoDream.Shared.Models
{
    /// <summary>
    /// 带偏移的入口
    /// </summary>
    public class ArchiveEntry: ReadOnlyEntry
    {

        public ArchiveEntry(string name, long offset, long length)
            : base(name, length)
        {
            Offset = offset;
        }

        public ArchiveEntry(
            string name, long offset, long length, long compressedLength,
            bool isEncrypted, DateTime? createdTime)
            : base(name, length, compressedLength, isEncrypted, createdTime)
        {
            Offset = offset;
        }

        public long Offset { get; private set; }

        /// <summary>
        /// 添加基础偏移
        /// </summary>
        /// <param name="offset"></param>
        public void Move(long offset)
        {
            Offset += offset;
        }

    }
}
