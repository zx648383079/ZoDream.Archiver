using ZoDream.Shared.Models;

namespace ZoDream.Shared.Bundle
{
    public interface IFileExporter
    {
        public string Name { get; }
        public void SaveAs(string fileName, ArchiveExtractMode mode);
    }
}
