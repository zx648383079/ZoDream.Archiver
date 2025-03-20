using System.Buffers;
using ZoDream.BundleExtractor.Unity.UI;
using ZoDream.KhronosExporter.Models;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    internal class RawExporter : IFileExporter
    {
        
        public RawExporter(TextAsset asset)
        {
            Name = asset.Name;
            // spine 的骨骼文件也是在这里，无法具体判断
            var buffer = ArrayPool<byte>.Shared.Rent(100);
            try
            {
                var len = asset.Script.Read(buffer, 0, buffer.Length);
                asset.Script.Position = 0;
                if (SpineExporter.IsSupport(buffer, len))
                {
                    _exporter = new SpineExporter(asset);
                    return;
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
            _exporter = new LuaExporter(asset);
        }

        private readonly IFileExporter _exporter;
        public string Name { get; private set; }

        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            _exporter.SaveAs(fileName, mode);
        }
    }
}
