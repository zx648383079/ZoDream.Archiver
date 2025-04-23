using ZoDream.Shared.Models;

namespace ZoDream.Shared.Bundle
{
    public interface IBundleExporter
    {
        public void SaveAs(string fileName, ArchiveExtractMode mode);
    }
}
