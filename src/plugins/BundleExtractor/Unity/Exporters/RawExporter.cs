using System.Buffers;
using System.IO;
using UnityEngine;
using ZoDream.LuaDecompiler;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    internal class RawExporter(int entryId, ISerializedFile resource) : IBundleExporter
    {
        public string FileName => resource[entryId].Name;

        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            if (resource[entryId] is not TextAsset asset) 
            {
                return;
            }
            // spine 的骨骼文件也是在这里，无法具体判断
            var buffer = ArrayPool<byte>.Shared.Rent(100);
            try
            {
                var input = asset.Script;
                var len = input.Read(buffer, 0, buffer.Length);
                input.Position = 0;
                if (buffer[0] is (byte)'[' or (byte)'{' && IsJson(asset.Script, buffer[0]))
                {
                    if (SpineExporter.IsSupport(buffer, len))
                    {
                        SpineExporter.SaveAs(asset, fileName, mode);
                        return;
                    }
                    SaveAs(input, fileName, ".json", mode);
                    return;
                }
                if (buffer.StartsWith(LuacReader.Signature))
                {
                    LuaExporter.SaveAs(new LuacReader(input), fileName, mode);
                    return;
                }
                if (buffer.StartsWith(LuaJitReader.Signature))
                {
                    LuaExporter.SaveAs(new LuaJitReader(input), fileName, mode);
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
                var extension = Path.GetExtension(fileName).ToLower();

                SaveAs(input, fileName, extension switch
                {
                    ".lua" or ".atlas" => extension,
                    _ => ".txt"
                }, mode);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        private static void SaveAs(Stream input, string fileName, string extension, ArchiveExtractMode mode)
        {
            if (!LocationStorage.TryCreate(fileName, extension, mode, out fileName))
            {
                return;
            }
            input.SaveAs(fileName);
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
