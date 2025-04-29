using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using ZoDream.Shared.Language;

namespace ZoDream.SourceGenerator
{
    public class PatternLanguageWriter(IEnumerable<TypeTreeNode> input) : ILanguageWriter
    {

        private readonly HashSet<string> _typeItems = [];
        private readonly Dictionary<string, string> _unknownItems = [];

        public void Write(Stream output)
        {
            var writer = new CodeWriter(output);
            Write(writer);
        }

        public void Write(ICodeWriter writer)
        {
            writer.WriteLine("#pragma author zodream")
                .WriteLine("#pragma description generate by source generator")
                .WriteLine("import zodream.io;");

            foreach (TypeTreeNode node in input)
            {
                WriteStruct(writer, node);
            }
        }

        private void WriteStruct(ICodeWriter writer, TypeTreeNode node)
        {
            var structName = node.Type;
            if (structName.StartsWith("PPtr<"))
            {
                structName = "PPtr";
            }
            WriteStruct(writer, structName, node.Children ?? []);
            _typeItems.Add(structName);
        }

        private void WriteStruct(ICodeWriter writer, string structName, TypeTreeNode[] children)
        {
            var maps = new Dictionary<string, string>();
            foreach (var item in children)
            {
                if (IsRegisterType(item.Type))
                {
                    continue;
                }
                if (item.Type == "vector")
                {
                    maps.Add(item.Name, WriteArray(writer, item));
                    continue;
                }
                WriteStruct(writer, item);
            }
            writer.WriteFormat("struct {0} {", structName).WriteIndentLine();
            foreach (var item in children)
            {
                if (maps.TryGetValue(item.Name, out var type))
                {
                    writer.WriteFormat("{0} {1};", type, item.Name).WriteLine(true);
                    continue;
                }
                WriteProperty(writer, item);
            }
            writer.WriteOutdentLine().Write('}').WriteLine(true);
        }



        private void WriteProperty(ICodeWriter writer, TypeTreeNode node)
        {
            writer.WriteFormat("{0} {1};", TraslateType(node.Type), node.Name).WriteLine(true);
        }

        private string WriteArray(ICodeWriter writer, TypeTreeNode node)
        {
            var children = node.Children?[0].Children?.Skip(1).ToArray() ?? [];
            var hash = string.Join(',', children.Select(i => i.Type));
            if (!_unknownItems.TryGetValue(hash, out var structName))
            {
                structName  = "Unknown_" + _unknownItems.Count;
                _unknownItems.Add(hash, structName);
                WriteStruct(writer, node.Type, children);
            }
            return $"zodream::List<{structName}>";
        }

        private bool IsRegisterType(string type)
        {
            if (type.StartsWith("PPtr<"))
            {
                type = "PPtr";
            }
            return _typeItems.Contains(type) || type is "int" or "float" 
                or "UInt8" or "bool" or "string" or "Matrix4x4f" 
                or "Vector3f" or "Vector4f" or "unsigned int" or "SInt64";
        }

        private static string TraslateType(string type)
        {
            if (type.StartsWith("PPtr<"))
            {
                return "PPtr";
            }
            return type switch
            {
                "UInt8" => "u8",
                "Matrix4x4f" => "zodream::Matrix4x4",
                "Vector3f" => "zodream::Vector3",
                "Vector4f" => "zodream::Vector4",
                "unsigned int" or "UInt32" => "u32",
                "SInt16" or "short" => "s16",
                "unsigned short" or "UInt16" => "u16",
                "int" or "SInt32" => "s32",
                "SInt64" or "long long" => "s64",
                "UInt64" or "unsigned long long" or "FileSize" => "u64",
                "string" => "zodream::AlignString",
                _ => type,
            };
        }
    }
}
