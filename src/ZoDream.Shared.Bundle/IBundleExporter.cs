using ZoDream.Shared.Models;

namespace ZoDream.Shared.Bundle
{
    public interface IBundleExporter
    {
        public string FileName { get; }
        public void SaveAs(string fileName, ArchiveExtractMode mode);
    }

    public interface IBundleConvertExporter
    {
        public void SaveAs(object instance, string fileName, ArchiveExtractMode mode);
    }
    public interface IBundleConvertExporter<T> : IBundleConvertExporter
    {
        public void SaveAs(T instance, string fileName, ArchiveExtractMode mode);
    }
}
