using UnityEngine;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Drawing;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    internal class NormalTextureExporter(Texture2D texture, ISerializedFile resource) : IBundleExporter
    {
        public string FileName => texture.Name;

        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            using var s = TextureExporter.ToImage(texture, resource);
            if (s is null)
            {
                return;
            }
            if (!fileName.EndsWith(".png"))
            {
                fileName += ".png";
            }
            using var v = BumpToNormal.Convert(s);
            v.SaveAs(fileName);
        }
    }
}
