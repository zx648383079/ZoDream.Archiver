using System.IO;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Archiver.ViewModels
{

    public abstract class MediaEntryStream(IEntryExplorer explorer, ISourceEntry entry) : IEntryStream
    {

        public string Name => entry.Name;

        public void SaveAs(Stream input)
        {
            explorer.SaveAs(entry, input);
        }
    }

    public class ImageEntryStream(IEntryExplorer explorer, ISourceEntry entry) : MediaEntryStream(explorer, entry)
    {
    }
}
