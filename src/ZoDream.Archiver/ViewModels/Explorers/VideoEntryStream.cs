using ZoDream.Shared.Interfaces;

namespace ZoDream.Archiver.ViewModels
{
    public class VideoEntryStream(IEntryExplorer explorer, ISourceEntry entry) : MediaEntryStream(explorer, entry)
    {
    }
}
