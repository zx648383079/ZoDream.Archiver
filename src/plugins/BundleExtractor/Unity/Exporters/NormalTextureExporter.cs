using System.IO;
using ZoDream.BundleExtractor.Unity.UI;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Drawing;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    internal class NormalTextureExporter(Texture2D texture) : IFileExporter
    {
        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            //if (!LocationStorage.TryCreate(fileName, ".png", mode, out fileName))
            //{
            //    return;
            //}
            using var s = texture.ToImage();
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
