using UnityEngine;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    internal class MovieExporter(int id, ISerializedFile resource) : IBundleExporter
    {
        public string FileName => resource[id].Name;

        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            if (resource[id] is not MovieTexture res)
            {
                return;
            }
            if (!LocationStorage.TryCreate(fileName, ".ogv", mode, out fileName))
            {
                return;
            }
            res.MovieData.SaveAs(fileName);
        }
    }
}
