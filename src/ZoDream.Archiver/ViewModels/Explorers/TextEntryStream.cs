using ZoDream.Shared.Interfaces;

namespace ZoDream.Archiver.ViewModels
{
    public class TextEntryStream(IEntryExplorer explorer, ISourceEntry entry) : MediaEntryStream(explorer, entry)
    {
    }
}
