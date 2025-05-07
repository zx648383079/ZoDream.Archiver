using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.Shared.Bundle
{
    public interface IRawEntryReader
    {

        public bool TryExtractTo(IReadOnlyEntry entry, string folder, ArchiveExtractMode mode);
    }
}
