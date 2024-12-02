using System.IO;
using System.Linq;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.RustWrapper;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal sealed class TextAsset(UIReader reader) : NamedObject(reader), IFileWriter
    {

        private static readonly byte[] LuacMagic = [0x1B, 0x4C, 0x75, 0x61];
        private static readonly byte[] LuaJitMagic = [0x1B, 0x4C, 0x4A];

        public Stream Script;


        public bool IsLua {
            get {
                if (Script is null || Script.Length < 5)
                {
                    return false;
                }
                var buffer = new byte[4];
                Script.ReadExactly(buffer);
                return buffer.SequenceEqual(LuacMagic) || buffer.Take(3).SequenceEqual(LuaJitMagic);
            }
        }

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
            if (!IsLua)
            {
                Script.SaveAs(fileName);
                return;
            }
            using var decompressor = new Compressor(CompressionID.Lua);
            using var fs = File.Create(fileName);
            decompressor.Decompress(Script, fs);
        }
    }
}
