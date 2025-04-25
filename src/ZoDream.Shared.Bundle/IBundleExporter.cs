using ZoDream.Shared.Models;

namespace ZoDream.Shared.Bundle
{
    public interface IBundleExporter
    {
        public string FileName { get; }
        public void SaveAs(string fileName, ArchiveExtractMode mode);
    }
}
