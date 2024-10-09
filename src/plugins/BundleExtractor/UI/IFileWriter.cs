using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.UI
{
    public interface IFileWriter
    {

        public void SaveAs(string fileName, ArchiveExtractMode mode);
    }
}
