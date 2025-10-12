using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.Document;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Document
{
    internal class DocumentConverter : BundleConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(VirtualDocument).IsAssignableFrom(objectType)
                || typeof(VirtualNode).IsAssignableFrom(objectType);
        }

        public override object? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var instance = Activator.CreateInstance(objectType);
            switch (instance)
            {
                case VirtualDocument doc:
                    Read(doc, reader);
                    break;
                case VirtualNode node:
                    Read(node, reader);
                    break;
                default:
                    break;
            }
            return instance;
        }

        public static void Read(VirtualDocument doc, IBundleBinaryReader reader)
        {
            doc.Version = reader.Get<Version>();
            var version = reader.Get<FormatVersion>();
            if (!IsFormat5(version))
            {
                var node = new VirtualNode();
                doc.Children = [node];
                Read(node, reader);
                return;
            }
            var nodesCount = reader.ReadInt32();
            if (nodesCount == 0)
            {
                doc.Children = [];
                return;
            }
            if (nodesCount < 0)
            {
                throw new InvalidDataException($"Node count cannot be negative: {nodesCount}");
            }

            var stringBufferSize = reader.ReadInt32();
            if (stringBufferSize < 0)
            {
                throw new InvalidDataException($"String buffer size cannot be negative: {stringBufferSize}");
            }

            var hasRef = HasRefTypeHash(version);
            var dataOffset = (24 + (hasRef ? 8 : 0)) * nodesCount;
            var pos = reader.Position;
            var customTypes = ReadCustomTypes(new PartialStream(reader.BaseStream, pos + dataOffset, stringBufferSize));
            reader.Position = pos;
            var items = new VirtualNode[nodesCount];
            for (int i = 0; i < nodesCount; i++)
            {
                var node = new VirtualNode
                {
                    Version = reader.ReadUInt16(),
                    Depth = reader.ReadByte(),
                    TypeFlags = reader.ReadByte(),
                    Type = GetTypeName(reader.ReadUInt32(), customTypes),
                    Name = GetTypeName(reader.ReadUInt32(), customTypes),
                    ByteSize = reader.ReadInt32(),
                    Index = reader.ReadInt32(),
                    MetaFlag = (TransferMetaFlags)reader.ReadUInt32()
                };
                if (hasRef)
                {
                    node.RefTypeHash = reader.ReadUInt64();
                }
                items[i] = node;
            }
            doc.Children = BuildTree(items);
            reader.Position += stringBufferSize;
        }

        /// <summary>
        /// 根据 level 建立层级关系
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public static VirtualNode[] BuildTree(VirtualNode[] items)
        {
            var offset = 0;
            return BuildTree(items, ref offset, out _);
        }

        private static VirtualNode[] BuildTree(VirtualNode[] items, ref int offset, out bool anyChildAlign)
        {
            anyChildAlign = false;
            if (offset >= items.Length)
            {
                return [];
            }
            var depth = items[offset].Depth;
            var res = new List<VirtualNode>()
            {
                items[offset]
            };
            if (res[0].MetaFlag.HasFlag(TransferMetaFlags.AlignBytes))
            {
                anyChildAlign = true;
            }
            while (++offset < items.Length)
            {
                var next = items[offset].Depth;
                if (next < depth)
                {
                    offset--;
                    break;
                }
                if (next > depth)
                {
                    var last = res.Last();
                    last.Children = BuildTree(items, ref offset, out var childIsAlign);
                    if (childIsAlign)
                    {
                        last.MetaFlag |= TransferMetaFlags.AnyChildUsesAlignBytes;
                        anyChildAlign = true;
                    }
                    continue;
                }
                res.Add(items[offset]);
                if (items[offset].MetaFlag.HasFlag(TransferMetaFlags.AlignBytes))
                {
                    anyChildAlign = true;
                }
            }
            return [.. res];
        }


        public static void Read(VirtualNode node, IBundleBinaryReader reader)
        {
            node.Type = reader.ReadStringZeroTerm();
            node.Name = reader.ReadStringZeroTerm();
            node.ByteSize = reader.ReadInt32();
            node.Index = reader.ReadInt32();
            node.TypeFlags = reader.ReadInt32();
            node.Version = reader.ReadInt32();
            node.MetaFlag = (TransferMetaFlags)reader.ReadUInt32();
            var nextDepth = (byte)(node.Depth + 1);
            node.Children = reader.ReadArray(_ => {
                var next = new VirtualNode()
                {
                    Depth = nextDepth
                };
                Read(next, reader);
                return next;
            });
        }

        private static Dictionary<uint, string> ReadCustomTypes(Stream input)
        {
            var res = new Dictionary<uint, string>();
            var reader = new EndianReader(input, EndianType.LittleEndian);
            while (reader.RemainingLength > 0)
            {
                res.Add((uint)reader.Position, reader.ReadStringZeroTerm());
            }
            return res;
        }

        private static string GetTypeName(uint value, Dictionary<uint, string> customTypes)
        {
            var isCustomType = (value & 0x80000000) == 0;
            if (isCustomType)
            {
                return customTypes[value];
            }
            uint offset = value & ~0x80000000;
            if (StringDefined.TryGetValue(offset, out string? nodeTypeName))
            {
                return nodeTypeName;
            }
            else
            {
                throw new Exception($"Unsupported asset class type name '{offset}''");
            }
        }

        /// <summary>
        /// 5.0.0a1 and greater<br/>
        /// Generation 10
        /// </summary>
        public static bool IsFormat5(FormatVersion generation) => generation >= FormatVersion.Unknown_10;
        /// <summary>
        /// 2019.1 and greater<br/>
        /// Generation 19
        /// </summary>
        public static bool HasRefTypeHash(FormatVersion generation) => generation >= FormatVersion.TypeTreeNodeWithTypeFlags;


        private static IReadOnlyDictionary<uint, string> StringDefined { get; } = new Dictionary<uint, string>
        {
            {0, "AABB"},
            {5, "AnimationClip"},
            {19, "AnimationCurve"},
            {34, "AnimationState"},
            {49, "Array"},
            {55, "Base"},
            {60, "BitField"},
            {69, "bitset"},
            {76, "bool"},
            {81, "char"},
            {86, "ColorRGBA"},
            {96, "Component"},
            {106, "data"},
            {111, "deque"},
            {117, "double"},
            {124, "dynamic_array"},
            {138, "FastPropertyName"},
            {155, "first"},
            {161, "float"},
            {167, "Font"},
            {172, "GameObject"},
            {183, "Generic Mono"},
            {196, "GradientNEW"},
            {208, "GUID"},
            {213, "GUIStyle"},
            {222, "int"},
            {226, "list"},
            {231, "long long"},
            {241, "map"},
            {245, "Matrix4x4f"},
            {256, "MdFour"},
            {263, "MonoBehaviour"},
            {277, "MonoScript"},
            {288, "m_ByteSize"},
            {299, "m_Curve"},
            {307, "m_EditorClassIdentifier"},
            {331, "m_EditorHideFlags"},
            {349, "m_Enabled"},
            {359, "m_ExtensionPtr"},
            {374, "m_GameObject"},
            {387, "m_Index"},
            {395, "m_IsArray"},
            {405, "m_IsStatic"},
            {416, "m_MetaFlag"},
            {427, "m_Name"},
            {434, "m_ObjectHideFlags"},
            {452, "m_PrefabInternal"},
            {469, "m_PrefabParentObject"},
            {490, "m_Script"},
            {499, "m_StaticEditorFlags"},
            {519, "m_Type"},
            {526, "m_Version"},
            {536, "Object"},
            {543, "pair"},
            {548, "PPtr<Component>"},
            {564, "PPtr<GameObject>"},
            {581, "PPtr<Material>"},
            {596, "PPtr<MonoBehaviour>"},
            {616, "PPtr<MonoScript>"},
            {633, "PPtr<Object>"},
            {646, "PPtr<Prefab>"},
            {659, "PPtr<Sprite>"},
            {672, "PPtr<TextAsset>"},
            {688, "PPtr<Texture>"},
            {702, "PPtr<Texture2D>"},
            {718, "PPtr<Transform>"},
            {734, "Prefab"},
            {741, "Quaternionf"},
            {753, "Rectf"},
            {759, "RectInt"},
            {767, "RectOffset"},
            {778, "second"},
            {785, "set"},
            {789, "short"},
            {795, "size"},
            {800, "SInt16"},
            {807, "SInt32"},
            {814, "SInt64"},
            {821, "SInt8"},
            {827, "staticvector"},
            {840, "string"},
            {847, "TextAsset"},
            {857, "TextMesh"},
            {866, "Texture"},
            {874, "Texture2D"},
            {884, "Transform"},
            {894, "TypelessData"},
            {907, "UInt16"},
            {914, "UInt32"},
            {921, "UInt64"},
            {928, "UInt8"},
            {934, "unsigned int"},
            {947, "unsigned long long"},
            {966, "unsigned short"},
            {981, "vector"},
            {988, "Vector2f"},
            {997, "Vector3f"},
            {1006, "Vector4f"},
            {1015, "m_ScriptingClassIdentifier"},
            {1042, "Gradient"},
            {1051, "Type*"},
            {1057, "int2_storage"},
            {1070, "int3_storage"},
            {1083, "BoundsInt"},
            {1093, "m_CorrespondingSourceObject"},
            {1121, "m_PrefabInstance"},
            {1138, "m_PrefabAsset"},
            {1152, "FileSize"},
            {1161, "Hash128"},
            {1169, "RenderingLayerMask"},
        };
    }
}
