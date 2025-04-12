using System.Buffers;
using System.IO;
using ZoDream.BundleExtractor.Unity.UI;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

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
                if (buffer[0] is (byte)'[' or (byte)'{' && IsJson(asset.Script, buffer[0]))
                {
                    if (SpineExporter.IsSupport(buffer, len))
                    {
                        _exporter = new SpineExporter(asset);
                        return;
                    }
                    _extension = ".json";
                }
                if (LuaExporter.IsSupport(buffer, len))
                {
                    _exporter = new LuaExporter(asset);
                    return;
                }
                // 识别暂无处理方法
                //if (VertexMapReader.IsSupport(buffer, len))
                //{
                //    if (BlendShapeReader.IsSupport(asset.Script))
                //    {
                //        // blend shape
                //        new BlendShapeReader().Read(asset.Script);
                //    } else if (asset.Name.Contains("_obj")) {
                //        // vertex
                //        new VertexMapReader().Read(asset.Script);
                //    }
                //}
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
            _input = asset.Script;
        }

        private readonly IFileExporter? _exporter;

        private readonly Stream? _input;
        private string _extension = string.Empty;
        public string Name { get; private set; }

        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            if (_exporter is not null)
            {
                _exporter.SaveAs(fileName, mode);
                return;
            }
            if (_input is null)
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(_extension))
            {
                _extension = Path.GetExtension(fileName) ?? ".txt";
            }
            if (!LocationStorage.TryCreate(fileName, _extension, mode, out fileName))
            {
                return;
            }
            _input.Position = 0;
            _input.SaveAs(fileName);
        }

        private static bool IsJson(Stream input, byte begin)
        {
            if (input.Length < 2)
            {
                return false;
            }
            input.Seek(-1, SeekOrigin.End);
            var last = input.ReadByte();
            input.Position = 0;
            return begin switch
            {
                (byte)'[' => last == ']',
                (byte)'{' => last == '}',
                _ => false
            };
        }

        private static bool IsJson(Stream input)
        {
            var code = input.ReadByte();
            if (code is '[' or '{')
            {
                return IsJson(input, (byte)code);
            }
            input.Position = 0;
            return false;
        }
    }
}
