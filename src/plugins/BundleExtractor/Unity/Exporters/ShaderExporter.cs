using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.BundleExtractor.Unity.UI;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    internal class ShaderExporter(Shader shader) : IFileExporter
    {
        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            if (!LocationStorage.TryCreate(fileName, ".shader", mode, out fileName))
            {
                return;
            }
            //File.WriteAllText(fileName, );
        }

        public void Dispose()
        {
        }
    }
}
