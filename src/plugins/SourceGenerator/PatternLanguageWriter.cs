using System.Collections.Generic;
using System.Diagnostics;
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
                .WriteLine("#pragma description generate by source generator");

            if (input is TypeNodeCollection c && !string.IsNullOrWhiteSpace(c.Version))
            {
                writer.WriteFormat("// Version {0}", c.Version).WriteLine();
            }

            writer.WriteLine()
                .WriteLine()
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
            if (IsRegisterType(structName))
            {
                return;
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
                if (item.Type == "map")
                {
                    maps.Add(item.Name, WriteMap(writer, item));
                    continue;
                }
                if (item.Type is "vector" or "set")
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
                    var field = item.Name.Replace(' ', '_');
                    writer.WriteFormat("{0} {1};", type, field).WriteLine(true);
                    continue;
                }
                WriteProperty(writer, item);
            }
            writer.WriteOutdentLine().Write('}').WriteLine(true);
        }



        private void WriteProperty(ICodeWriter writer, TypeTreeNode node)
        {
            writer.WriteFormat("{0} {1};", TranslateType(node.Type), node.Name.Replace(' ', '_')).WriteLine(true);
        }

        private string WriteArray(ICodeWriter writer, TypeTreeNode node)
        {
            var children = node.Children?[0].Children?.Skip(1).ToArray() ?? [];
            if (children.Length == 1)
            {
                WriteStruct(writer, children[0]);
                return $"zodream::List<{TranslateType(children[0].Type)}>";
            }
            var hash = string.Join(',', children.Select(i => i.Type));
            if (!_unknownItems.TryGetValue(hash, out var structName))
            {
                structName  = "Unknown_" + _unknownItems.Count;
                _unknownItems.Add(hash, structName);
                WriteStruct(writer, structName, children);
            }
            return $"zodream::List<{structName}>";
        }

        private string WriteMap(ICodeWriter writer, TypeTreeNode node)
        {
            var children = node.Children?[0].Children?[1].Children?.ToArray() ?? [];
            Debug.Assert(children.Length == 2);
            foreach (var item in children)
            {
                WriteStruct(writer, item);
            }
            return $"zodream::Map<{string.Join(", ", children.Select(i => TranslateType(i.Type)))}>";
        }

        private bool IsRegisterType(string type)
        {
            if (type.StartsWith("PPtr<"))
            {
                type = "PPtr";
            }
            return _typeItems.Contains(type) || type is "int" or "float"
                or "UInt8" or "bool" or "string" or "Matrix4x4f" or "Quaternionf"
                or "Vector2f" or "Vector3f" or "Vector4f"
                or "unsigned int" or "SInt64" or "UInt64" or "UInt16"
                or "SInt16" or "SInt8" or "FileSize" or "Type*" or "None"
                or "GUID" or "Hash128";
        }

        private static string TranslateType(string type)
        {
            if (type.StartsWith("PPtr<"))
            {
                return "PPtr";
            }
            return type switch
            {
                "None" => "nil",
                "UInt8" => "u8",
                "SInt8" => "s8",
                "Matrix4x4f" => "zodream::Matrix4x4",
                "Vector2f" => "zodream::Vector2",
                "Vector3f" => "zodream::Vector3",
                "Vector4f" => "zodream::Vector4",
                "Quaternionf" => "zodream::Quaternion",
                "unsigned int" or "UInt32" or "Type*" => "u32",
                "SInt16" or "short" => "s16",
                "unsigned short" or "UInt16" => "u16",
                "int" or "SInt32" => "s32",
                "SInt64" or "long long" => "s64",
                "UInt64" or "unsigned long long" or "FileSize" => "u64",
                "string" => "zodream::AlignString",
                "GUID" or "Hash128" => "Array<u8, 16>",
                _ => type,
            };
        }
    }
}
