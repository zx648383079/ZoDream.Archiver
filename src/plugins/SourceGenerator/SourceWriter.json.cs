using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using ZoDream.Shared.Language;

namespace ZoDream.SourceGenerator
{
    public partial class TypeSourceWriter(IEnumerable<TypeTreeNode> input) : ILanguageWriter
    {
        private readonly ICodeWriter _cvtWriter = new CodeWriter();
        private readonly HashSet<string> _typeItems = [];
        private readonly Dictionary<string, string> _unknownItems = [];
        private readonly HashSet<string> _cvtItems = [];

        public void Write(Stream output)
        {
            var writer = new CodeWriter(output);
            Write(writer);
            _cvtWriter.Dispose();
        }

        public void Write(ICodeWriter writer)
        {
            if (input is TypeNodeCollection c && !string.IsNullOrWhiteSpace(c.Version))
            {
                writer.WriteFormat("// Version {0}", c.Version).WriteLine();
            }

            writer.WriteLine()
                .WriteLine();

            foreach (TypeTreeNode node in input)
            {
                WriteStruct(writer, node);
            }
            WriteRegister(writer);
        }

        /// <summary>
        /// 合并数据并追加
        /// </summary>
        /// <param name="writer"></param>
        private void WriteRegister(ICodeWriter writer)
        {
            writer.Write(_cvtWriter);
            writer.WriteLine(true)
                .Write("internal partial static class Engine").WriteLine(true)
                .Write('{').WriteIndentLine()
                .Write("public static IBundleConverter[] Converters = [").WriteIndentLine();
            foreach (var item in _cvtItems)
            {
                writer.WriteFormat("new {0}Converter(),", item).WriteLine(true);
            }
            writer.WriteOutdentLine()
                .Write("];").WriteOutdentLine()
                .Write('}').WriteLine(true);
        }

        /// <summary>
        /// 生成类型
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        private KeyValuePair<string, string> WriteType(ICodeWriter writer, TypeTreeNode node)
        {
            if (IsRegisterType(node.Type))
            {
                return new(TranslateType(node.Type), TranslateReadType(node.Type));
            }
            if (node.Type == "xform")
            {
                var value = $"Transform<{TranslateType(node.Children[0].Type)}>";
                return new(value, TranslateReadType(value));
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
            return new(TranslateType(node.Type), TranslateReadType(node.Type));
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
            var maps = new Dictionary<string, KeyValuePair<string, string>>();
            var arrayItems = new Dictionary<string, int>();
            foreach (var item in children)
            {
                var name = item.Name;
                var match = ArrayIndexRegex().Match(name);
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
            writer.WriteFormat("internal struct {0}", structName).WriteLine(true)
                .Write("{").WriteIndentLine();
            _cvtItems.Add(structName);
            _cvtWriter.WriteFormat("internal struct {0}Converter : BundleConverter<{0}>", structName).WriteLine(true)
                .Write("{")
                .WriteIndentLine()
                .WriteFormat("public override {0} Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)", structName)
                .WriteLine(true)
                .Write("{")
                .WriteIndentLine()
                .WriteFormat("var res = new {0}();", structName).WriteLine(true);
            foreach (var item in children)
            {
                var name = item.Name;
                var match = ArrayIndexRegex().Match(name); 
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
                        writer.WriteFormat("public {0}[] {1};", type.Key, field).WriteLine(true);
                        _cvtWriter.WriteFormat("res.{0} = reader.ReadArray({1}, _ => {2});",
                            field, arrayItems[name] + 1, type.Value).WriteLine(true);
                        continue;
                    }
                    writer.WriteFormat("public {0} {1};", type.Key, field).WriteLine(true);
                    _cvtWriter.WriteFormat("res.{0} = {1};",
                        field, type.Value).WriteLine(true);
                    continue;
                }
                WriteProperty(writer, item);
            }
            writer.WriteOutdentLine().Write("}").WriteLine(true);
            _cvtWriter.Write("return res;")
                .WriteOutdentLine().Write("}")
                .WriteOutdentLine().Write("}").WriteLine(true);
        }



        private void WriteProperty(ICodeWriter writer, TypeTreeNode node)
        {
            var field = node.Name.Replace(' ', '_');
            writer.WriteFormat("public {0} {1};", TranslateType(node.Type), field).WriteLine(true);
            _cvtWriter.WriteFormat("res.{0} = {1};",
                    field, TranslateReadType(node.Type)).WriteLine(true);
        }

        private KeyValuePair<string, string> WriteArray(ICodeWriter writer, TypeTreeNode node)
        {
            if (node.Type != "Array" && node.Children?.Length == 1 && node.Children[0].Type == "Array")
            {
                node = node.Children[0];
            }
            var children = node?.Children?.Skip(1).ToArray() ?? [];
            if (children.Length == 1)
            {
                var res = WriteType(writer, children[0]);
                return new($"{res.Key}[]", $"reader.ReadArray(_ => {res.Value})");
            }
            var hash = string.Join(',', children.Select(i => i.Type));
            if (!_unknownItems.TryGetValue(hash, out var structName))
            {
                structName = "Unknown_" + _unknownItems.Count;
                _unknownItems.Add(hash, structName);
                WriteStruct(writer, structName, children);
            }
            return new($"{structName}[]", $"reader.ReadArray<{structName}>(serializer)");
        }

        private KeyValuePair<string, string> WriteMap(ICodeWriter writer, TypeTreeNode node)
        {
            var res = WriteMapPair(writer, node.Children?[0].Children?[1]);
            return new($"{res.Key}[]", $"reader.ReadArray(_ => {res.Value})");
        }

        private KeyValuePair<string, string> WriteMapPair(ICodeWriter writer, TypeTreeNode node)
        {
            var children = node.Children?.ToArray() ?? [];
            Debug.Assert(children.Length == 2);
            var key = string.Empty;
            var value = string.Empty;
            for (int i = 0; i < children.Length; i++)
            {
                var res = WriteType(writer, children[i]);
                if (i > 0)
                {
                    key += ", ";
                    value += ", ";
                }
                key += res.Key;
                value += res.Value;
            }
            return new($"KeyValuePair<{key}>", $"new KeyValuePair<{key}>({value})");
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
                or "GUID" or "Hash128" 
                or "float3" or "float4" or "Rectf";
        }

        private static string TranslateType(string type)
        {
            if (type.StartsWith("PPtr<"))
            {
                return "I" + type;
            }
            return type switch
            {
                "None" => "null",
                "UInt8" => "byte",
                "SInt8" => "sbyte",
                "Matrix4x4f" => "Matrix4x4",
                "Vector2f" => "Vector2",
                "Vector3f" or "float3" => "Vector3",
                "Vector4f" or "float4" => "Vector4",
                "Rectf" => "Rect",
                "Quaternionf" => "Quaternion",
                "unsigned int" or "UInt32" or "Type*" => "uint",
                "SInt16" or "short" => "short",
                "unsigned short" or "UInt16" => "ushort",
                "int" or "SInt32" => "int",
                "SInt64" or "long long" => "long",
                "UInt64" or "unsigned long long" or "FileSize" => "ulong",
                "GUID" or "Hash128" => "byte[]",
                _ => type,
            };
        }

        private static string TranslateReadType(string text)
        {
            if (text.StartsWith("PPtr<"))
            {
                return $"reader.Read{text}(serializer)";
            }
            return text switch
            {
                "None" => "null",
                "UInt8" => "reader.ReadByte()",
                "SInt8" => "reader.ReadSByte()",
                "Matrix4x4f" => "reader.ReadMatrix()",
                "Vector2f" => "reader.Vector2()",
                "Vector3f" or "float3" => "reader.Vector3()",
                "Vector4f" or "float4" => "reader.Vector4()",
                "Rectf" => "reader.Rect()",
                "Quaternionf" => "reader.Quaternion()",
                "Vector3" or "Vector2" or "Vector4" or "Quaternion" => $"reader.Read{text}()",
                "unsigned int" or "UInt32" or "Type*" => "reader.ReadUInt32()",
                "SInt16" or "short" => "reader.ReadInt16()",
                "unsigned short" or "UInt16" => "reader.ReadUInt16()",
                "int" or "SInt32" => "reader.ReadInt32()",
                "SInt64" or "long long" => "reader.ReadInt64()",
                "UInt64" or "unsigned long long" or "FileSize" => "reader.ReadInt64()",
                "string" => "reader.ReadAlignedString()",
                "float" => "reader.ReadSingle()",
                "double" => "reader.ReadDouble()",
                "bool" => "reader.ReadBoolean()",
                "GUID" or "Hash128" => "reader.ReadBytes(16)",
                "Transform<Vector3>" => "reader.ReadXForm()",
                "Transform<Vector4>" => "reader.ReadXForm4()",
                _ => $"serializer.Deserialize<{text}>(reader)"
            };
        }

        [GeneratedRegex(@"(.+)\[(\d+)\]$")]
        internal static partial Regex ArrayIndexRegex();
    }
}
