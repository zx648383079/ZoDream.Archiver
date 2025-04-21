using ZoDream.Shared.Models;

namespace ZoDream.Shared.Bundle
{
    public interface IBundleExporter
    {
        public string Name { get; }
        public void SaveAs(string fileName, ArchiveExtractMode mode);
    }
}
