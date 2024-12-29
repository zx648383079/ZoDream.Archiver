using ZoDream.Shared.Interfaces;

namespace ZoDream.Archiver.ViewModels
{
    public class AudioEntryStream(IEntryExplorer explorer, ISourceEntry entry) : MediaEntryStream(explorer, entry)
    {
    }
}
