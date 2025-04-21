using System;
using System.Text;
using ZoDream.BundleExtractor.Unity.UI;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    internal class SpineExporter(TextAsset asset) : IBundleExporter
    {
        public string Name => asset.Name;
        public void SaveAs(string fileName, ArchiveExtractMode mode)
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
    }
}
