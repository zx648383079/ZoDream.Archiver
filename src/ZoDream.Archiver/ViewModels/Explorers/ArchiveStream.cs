using ZoDream.Shared.Interfaces;

namespace ZoDream.Archiver.ViewModels
{
    public class ArchiveEntryStream(IEntryExplorer source) : DirectoryEntryStream
    {
        public IEntryExplorer Archive => source;
    }
}
