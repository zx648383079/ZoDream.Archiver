using System.Buffers;
using System.IO;
using ZoDream.BundleExtractor.Unity.Exporters;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal sealed class TextAsset(UIReader reader) : NamedObject(reader), IFileExporter
    {
        public Stream Script;
        public override void Read(IBundleBinaryReader reader)
        {
            base.Read(reader);
            Script = reader.ReadAsStream();
        }

        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            new RawExporter(this).SaveAs(fileName, mode);
        }
    }
}
