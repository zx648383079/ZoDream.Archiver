using System.Text;
using UnityEngine;
using ZoDream.Shared.Bundle;
using NodeItem = UnityEngine.TypeTreeNode;

namespace ZoDream.BundleExtractor.Unity.SerializedFiles
{
    public class TypeTreeNode : NodeItem
    {
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

        public TypeTreeNode() { }

        public TypeTreeNode(string type, string name, int level, bool align)
        {
            Type = type;
            Name = name;
            Level = (byte)level;
            MetaFlag = align ? TransferMetaFlags.AlignBytes : TransferMetaFlags.NoTransferFlags;
        }

        public TypeTreeNode(string type, string name, int level, int byteSize, int index, int version, int typeFlags, TransferMetaFlags metaFlag)
        {
            Type = type;
            Name = name;
            Level = (byte)level;
            ByteSize = byteSize;
            Index = index;
            Version = version;
            TypeFlags = typeFlags;
            MetaFlag = metaFlag;
        }

        /// <summary>
        /// Type offset in <see cref="TypeTree.StringBuffer"/>
        /// </summary>
        public uint TypeStrOffset { get; set; }
        /// <summary>
        /// Name offset in <see cref="TypeTree.StringBuffer"/>
        /// </summary>
        public uint NameStrOffset { get; set; }

        public void Read(IBundleBinaryReader reader)
        {
            var version = reader.Get<FormatVersion>();
            if (IsFormat5(version))
            {
                Version = reader.ReadUInt16();
                Level = reader.ReadByte();
                TypeFlags = reader.ReadByte();
                TypeStrOffset = reader.ReadUInt32();
                NameStrOffset = reader.ReadUInt32();
                ByteSize = reader.ReadInt32();
                Index = reader.ReadInt32();
                MetaFlag = (TransferMetaFlags)reader.ReadUInt32();
                if (HasRefTypeHash(version))
                {
                    RefTypeHash = reader.ReadUInt64();
                }
            }
            else
            {
                Type = reader.ReadStringZeroTerm();
                Name = reader.ReadStringZeroTerm();
                ByteSize = reader.ReadInt32();
                Index = reader.ReadInt32();
                TypeFlags = reader.ReadInt32();
                Version = reader.ReadInt32();
                MetaFlag = (TransferMetaFlags)reader.ReadUInt32();
            }
        }

        public override string? ToString()
        {
            if (Type == null)
            {
                return base.ToString();
            }
            else
            {
                return $"{Type} {Name}";
            }
        }

        public StringBuilder ToString(StringBuilder sb)
        {
            sb.Append('\t', Level).Append(Type).Append(' ').Append(Name);
            sb.AppendFormat(" // ByteSize{0}{1:x}{2}, Index{3}{4:x}{5}, Version{6}{7:x}{8}, IsArray{{{9}}}, MetaFlag{10}{11:x}{12}",
                    "{", ByteSize, "}",
                    "{", Index, "}",
                    "{", Version, "}",
                    TypeFlags,
                    "{", (uint)MetaFlag, "}");
            return sb;
        }


    }
}
