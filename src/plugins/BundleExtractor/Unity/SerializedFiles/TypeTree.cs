using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.SerializedFiles
{
    public sealed class TypeTree
    {
        public void Read(EndianReader reader, FormatVersion version)
        {
            if (TypeTreeNode.IsFormat5(version))
            {
                IsFormat5 = true;

                int nodesCount = reader.ReadInt32();
                if (nodesCount < 0)
                {
                    throw new InvalidDataException($"Node count cannot be negative: {nodesCount}");
                }

                int stringBufferSize = reader.ReadInt32();
                if (stringBufferSize < 0)
                {
                    throw new InvalidDataException($"String buffer size cannot be negative: {stringBufferSize}");
                }

                Nodes.Clear();
                Nodes.Capacity = nodesCount;
                for (int i = 0; i < nodesCount; i++)
                {
                    var node = new TypeTreeNode();
                    node.Read(reader, version);
                    Nodes.Add(node);
                }
                if (stringBufferSize == 0)
                {
                    StringBuffer = [];
                }
                else
                {
                    StringBuffer = new byte[stringBufferSize];
                    reader.Read(StringBuffer, 0, StringBuffer.Length);
                }
                SetNamesFromBuffer();
            }
            else
            {
                IsFormat5 = false;
                Nodes.Clear();
                ReadTreeNode(reader, version, Nodes, 0);
            }
        }

        private static void ReadTreeNode(EndianReader reader, FormatVersion version,
            ICollection<TypeTreeNode> nodes, byte depth)
        {
            var node = new TypeTreeNode();
            node.Read(reader, version);
            node.Level = depth;
            nodes.Add(node);

            int childCount = reader.ReadInt32();
            for (int i = 0; i < childCount; i++)
            {
                ReadTreeNode(reader, version, nodes, (byte)(depth + 1));
            }
        }

        public override string? ToString()
        {
            if (Nodes == null)
            {
                return base.ToString();
            }

            return Nodes[0].ToString();
        }

        public StringBuilder ToString(StringBuilder sb)
        {
            if (Nodes != null)
            {
                foreach (TypeTreeNode node in Nodes)
                {
                    node.ToString(sb).AppendLine();
                }
            }
            return sb;
        }

        private int GetChildCount(int index)
        {
            var count = 0;
            var depth = Nodes[index].Level + 1;
            for (var i = index + 1; i < Nodes.Count; i++)
            {
                var nodeDepth = Nodes[i].Level;
                if (nodeDepth < depth)
                {
                    break;
                }
                if (nodeDepth == depth)
                {
                    count++;
                }
            }
            return count;
        }

        public string Dump
        {
            get
            {
                var sb = new StringBuilder();
                ToString(sb);
                return sb.ToString();
            }
        }

        private void SetNamesFromBuffer()
        {
            Debug.Assert(IsFormat5);
            var customTypes = new Dictionary<uint, string>();
            using (var stream = new MemoryStream(StringBuffer))
            {
                using var reader = new EndianReader(stream, EndianType.LittleEndian);
                while (stream.Position < stream.Length)
                {
                    uint position = (uint)stream.Position;
                    string name = reader.ReadStringZeroTerm();
                    customTypes.Add(position, name);
                }
            }

            foreach (var node in Nodes)
            {
                node.Type = GetTypeName(customTypes, node.TypeStrOffset);
                node.Name = GetTypeName(customTypes, node.NameStrOffset);
            }
        }

        private static string GetTypeName(Dictionary<uint, string> customTypes, uint value)
        {
            var isCustomType = (value & 0x80000000) == 0;
            if (isCustomType)
            {
                return customTypes[value];
            }
            else
            {
                uint offset = value & ~0x80000000;
                if (CommonString.StringBuffer.TryGetValue(offset, out string? nodeTypeName))
                {
                    return nodeTypeName;
                }
                else
                {
                    throw new Exception($"Unsupported asset class type name '{offset}''");
                }
            }
        }

        public List<TypeTreeNode> Nodes { get; } = [];
        public byte[] StringBuffer { get; set; } = [];
        /// <summary>
        /// 5.0.0a1 and greater<br/>
        /// Generation 10
        /// </summary>
        private bool IsFormat5 { get; set; }
    }
}
