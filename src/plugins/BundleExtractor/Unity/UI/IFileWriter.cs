using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.UI
{
    public interface IFileWriter
    {

        public void SaveAs(string fileName, ArchiveExtractMode mode);
    }
}
