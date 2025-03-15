using ZoDream.Shared.Models;

namespace ZoDream.Shared.Interfaces
{
    public interface IArchiveExtractOptions
    {
        public string OutputFolder { get; }

        public ArchiveExtractMode FileMode { get; }
    }
}
