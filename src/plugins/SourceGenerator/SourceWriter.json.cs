using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using ZoDream.Shared.Language;

namespace ZoDream.SourceGenerator
{
    public class TypeSourceWriter(IEnumerable<TypeTreeNode> input) : ILanguageWriter
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
        private string WriteType(ICodeWriter writer, TypeTreeNode node)
        {
            if (IsRegisterType(node.Type))
            {
                return node.Type;
            }
            if (node.Type == "xform")
            {
                return $"Transform<{TranslateType(node.Children[0].Type)}>";
            }
            if (node.Type == "map")
            {
                return WriteMap(writer, node);
            }
            if (node.Type is "vector" or "set" or "staticvector")
            {
                return WriteArray(writer, node);
            }
            WriteStruct(writer, node);
            return node.Type;
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
                maps.Add(item.Name, WriteType(writer, item));
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
                if (maps.TryGetValue(item.Name, out var type))
                {
                    var field = item.Name.Replace(' ', '_');
                    if (item.Type == "map")
                    {
                        var keys = type.Split(';');
                        var targetType = string.Join(", ", keys.Select(TranslateType));
                        writer.WriteFormat("public KeyValuePair<{0}>[] {1};", targetType, field).WriteLine(true);
                        _cvtWriter.WriteFormat("res.{0} = reader.ReadArray(_ => new KeyValuePair<{1}>({2}));",
                            field, targetType, string.Join(", ", keys.Select(TranslateReadType))).WriteLine(true);
                        continue;
                    }
                    if (item.Type is "vector" or "set" or "staticvector")
                    {
                        writer.WriteFormat("public {0}[] {1};", TranslateType(type), field).WriteLine(true);
                        _cvtWriter.WriteFormat("res.{0} = reader.ReadArray(_ => {1});",
                            field, TranslateReadType(type)).WriteLine(true);
                        continue;
                    }
                    writer.WriteFormat("public {0} {1};", TranslateType(type), field).WriteLine(true);
                    _cvtWriter.WriteFormat("res.{0} = {1};",
                        field, TranslateReadType(type)).WriteLine(true);
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

        private string WriteArray(ICodeWriter writer, TypeTreeNode node)
        {
            var children = node.Children?[0].Children?.Skip(1).ToArray() ?? [];
            if (children.Length == 1)
            {
                return WriteType(writer, children[0]);
            }
            var hash = string.Join(',', children.Select(i => i.Type));
            if (!_unknownItems.TryGetValue(hash, out var structName))
            {
                structName = "Unknown_" + _unknownItems.Count;
                _unknownItems.Add(hash, structName);
                WriteStruct(writer, structName, children);
            }
            return $"{structName}";
        }

        private string WriteMap(ICodeWriter writer, TypeTreeNode node)
        {
            var children = node.Children?[0].Children?[1].Children?.ToArray() ?? [];
            Debug.Assert(children.Length == 2);
            return string.Join(';', children.Select(i => WriteType(writer, i)));
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
                or "float3" or "float4";
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
    }
}
