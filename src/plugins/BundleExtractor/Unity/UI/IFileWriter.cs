using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal interface IFileWriter
    {

        public void SaveAs(string fileName, ArchiveExtractMode mode);
    }
}
