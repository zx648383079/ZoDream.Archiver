using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine.Document;
using ZoDream.Shared;
using ZoDream.Shared.Language;

namespace ZoDream.SourceGenerator
{
    public class PatternLanguageWriter(IEnumerable<VirtualNode> input) : ILanguageWriter
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

            if (input is VirtualDocument c)
            {
                writer.WriteFormat("// Version {0}", c.Version).WriteLine();
            }

            writer.WriteLine()
                .WriteLine()
                .WriteLine("import zodream.io;");

            foreach (VirtualNode node in input)
            {
                WriteStruct(writer, node);
            }
        }

        private string WriteType(ICodeWriter writer, VirtualNode node)
        {
            if (IsRegisterType(node.Type))
            {
                return TranslateType(node.Type);
            }
            if (node.Type == "xform")
            {
                return $"Transform<{TranslateType(node.Children[0].Type)}>";
            }
            if (node.Type == "pair")
            {
                return WriteMapPair(writer, node);
            }
            if (node.Type == "map")
            {
                return WriteMap(writer, node);
            }
            if (node.Type is "vector" or "set" or "staticvector" or "Array")
            {
                return WriteArray(writer, node);
            }
            WriteStruct(writer, node);
            return TranslateType(node.Type);
        }

      

        private void WriteStruct(ICodeWriter writer, VirtualNode node)
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

        private void WriteStruct(ICodeWriter writer, string structName, VirtualNode[] children)
        {
            var maps = new Dictionary<string, string>();
            var arrayItems = new Dictionary<string, int>();
            foreach (var item in children)
            {
                var name = item.Name;
                var match = TypeSourceWriter.ArrayIndexRegex().Match(name);
                if (match.Success)
                {
                    name = match.Groups[1].Value;
                    arrayItems[name] = int.Parse(match.Groups[2].Value);
                }
                if (maps.ContainsKey(name))
                {
                    continue;
                }
                maps.Add(name, WriteType(writer, item));
            }
            writer.WriteFormat("struct {0} {{", structName).WriteIndentLine();
            foreach (var item in children)
            {
                var name = item.Name;
                var match = TypeSourceWriter.ArrayIndexRegex().Match(name);
                if (match.Success)
                {
                    name = match.Groups[1].Value;
                    if (item.Name != $"{name}[{arrayItems[name]}]")
                    {
                        continue;
                    }
                }
                if (maps.TryGetValue(name, out var type))
                {
                    var field = name.Replace(' ', '_');
                    if (match.Success)
                    {
                        writer.WriteFormat("Array<{0}, {1}> {2};", type, arrayItems[name] + 1, field).WriteLine(true);
                        continue;
                    }
                    writer.WriteFormat("{0} {1};", type, field).WriteLine(true);
                    continue;
                }
                WriteProperty(writer, item);
            }
            writer.WriteOutdentLine().Write('}').WriteLine(true);
        }



        private void WriteProperty(ICodeWriter writer, VirtualNode node)
        {
            writer.WriteFormat("{0} {1};", TranslateType(node.Type), node.Name.Replace(' ', '_')).WriteLine(true);
        }

        private string WriteArray(ICodeWriter writer, VirtualNode node)
        {
            if (node.Type != "Array" && node.Children?.Length == 1 && node.Children[0].Type == "Array")
            {
                node = node.Children[0];
            }
            var children = node?.Children?.Skip(1).ToArray() ?? [];
            if (children.Length == 1)
            {
                return $"zodream::List<{WriteType(writer, children[0])}>";
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

        private string WriteMap(ICodeWriter writer, VirtualNode node)
        {
            var children = node.Children?[0].Children?[1].Children?.ToArray() ?? [];
            Expectation.ThrowIfNot(children.Length == 2);
            return $"zodream::Map<{string.Join(", ", children.Select(i => WriteType(writer, i)))}>";
        }

        private string WriteMapPair(ICodeWriter writer, VirtualNode node)
        {
            var children = node.Children?.ToArray() ?? [];
            Expectation.ThrowIfNot(children.Length == 2);
            return $"zodream::MapPair<{string.Join(", ", children.Select(i => WriteType(writer, i)))}>";
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
                or "GUID" or "Hash128" or "float3" or "float4" or "Rectf";
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
                "Vector3f" or "float3" => "zodream::Vector3",
                "Vector4f" or "float4" => "zodream::Vector4",
                "Quaternionf" => "zodream::Quaternion",
                "unsigned int" or "UInt32" or "Type*" => "u32",
                "SInt16" or "short" => "s16",
                "unsigned short" or "UInt16" => "u16",
                "int" or "SInt32" => "s32",
                "SInt64" or "long long" => "s64",
                "UInt64" or "unsigned long long" or "FileSize" => "u64",
                "string" => "zodream::AlignString",
                "GUID" or "Hash128" => "Array<u8, 16>",

                "Rectf" => "zodream::Rect",
                "Vector3" or "Vector2" or "Vector4" or "Quaternion" => $"zodream::{type}",
                _ => type,
            };
        }
    }
}
