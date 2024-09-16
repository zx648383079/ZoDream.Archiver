using SharpCompress.Common;
using SharpCompress.Readers;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.Shared.Compression
{
    public static class CompressHelper
    {
        public static ReaderOptions? Convert(IArchiveOptions? options)
        {
            if (options is null)
            {
                return null;
            }
            return new ReaderOptions()
            {
                LeaveStreamOpen = options.LeaveStreamOpen,
                LookForHeader = options.LookForHeader,
                Password = options.Password,
            };
        }

        public static IReadOnlyEntry Convert(IEntry item)
        {
            return new ReadOnlyEntry(item.Key ?? string.Empty, item.Size, item.CompressedSize, item.IsEncrypted, item.LastModifiedTime);
        }

        
    }
}
