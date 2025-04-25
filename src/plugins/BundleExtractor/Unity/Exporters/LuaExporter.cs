using System;
using System.IO;
using UnityEngine;
using ZoDream.LuaDecompiler;
using ZoDream.LuaDecompiler.Models;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;
using ZoDream.Shared.Language;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    internal class LuaExporter(int entryId, ISerializedFile resource) : IBundleExporter
    {
        public string FileName => resource[entryId].Name;
        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            var asset = resource[entryId] as TextAsset;
            asset.Script.Position = 0;
            var extension = Path.GetExtension(fileName);
            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = ".txt";
            }
            var decompressor = new LuaScheme();
            var res = decompressor.Open(asset.Script);
            if (res is null)
            {
                if (!LocationStorage.TryCreate(fileName, extension, mode, out fileName))
                {
                    return;
                }
                asset.Script.SaveAs(fileName);
                return;
            }
            if (!LocationStorage.TryCreate(fileName, ".lua", mode, out fileName))
            {
                return;
            }
            using var fs = File.Create(fileName);
            decompressor.Create(fs, res);
        }

        public static void SaveAs(ILanguageReader<LuaBytecode> reader, string fileName, ArchiveExtractMode mode)
        {
            if (!LocationStorage.TryCreate(fileName, ".lua", mode, out fileName))
            {
                return;
            }
            var data = reader.Read();
            using var fs = File.Create(fileName);
            new LuaWriter(data).Write(fs);
        }
        internal static bool IsSupport(byte[] buffer, int length)
        {
            if (buffer.StartsWith(LuacReader.Signature))
            {
                return true;
            }
            if (buffer.StartsWith(LuaJitReader.Signature))
            {
                return true;
            }
            return false;
        }
    }
}
