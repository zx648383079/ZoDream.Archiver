using System;
using System.Text;
using UnityEngine;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    internal class SpineExporter(int entryId, ISerializedFile resource) : IMultipartExporter
    {
        public string FileName => resource[entryId].Name;

        public string SourcePath => resource.FullPath;

        public bool IsEmpty => false;

        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            if (resource[entryId] is not TextAsset asset)
            {
                return;
            }
            SaveAs(asset, fileName, mode);
        }
        public static void SaveAs(TextAsset asset, string fileName, ArchiveExtractMode mode)
        {
            if (fileName.EndsWith(".skel"))
            {
                fileName = fileName[..^5];
            }
            if (!LocationStorage.TryCreate(fileName, ".skel.json", mode, out fileName))
            {
                return;
            }
            asset.Script.Position = 0;
            asset.Script.SaveAs(fileName);
        }

        public static bool IsSupport(byte[] buffer, int count)
        {
            if (count < 10)
            {
                return false;
            }
            return buffer[0] == '{' && buffer.IndexOf(Encoding.ASCII.GetBytes("\"skeleton\"")) > 0;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
