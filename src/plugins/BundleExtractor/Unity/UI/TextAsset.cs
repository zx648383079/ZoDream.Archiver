using System.IO;
using ZoDream.LuaDecompiler;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

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
            var extension = Path.GetExtension(fileName);
            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = ".txt";
            }
            if (!LocationStorage.TryCreate(fileName, extension, mode, out fileName))
            {
                return;
            }
            var decompressor = new LuaScheme();
            var res = decompressor.Open(Script, string.Empty, string.Empty);
            if (res is null)
            {
                Script.SaveAs(fileName);
                return;
            }
            using var fs = File.Create(fileName);
            decompressor.Create(fs, res);
        }
    }
}
