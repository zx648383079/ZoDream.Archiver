using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.AutodeskExporter.Models;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;

namespace ZoDream.AutodeskExporter
{
    internal class FbxReader
    {
        public object? Read(Stream input)
        {
            var reader = new BundleBinaryReader(input, EndianType.LittleEndian);
            var header = new FbxHeader();
            Debug.Assert(reader.ReadBytes(FbxHeader.Signature.Length).Equal(FbxHeader.Signature));
            header.Version = reader.ReadUInt32();
            if (header.Version < 7500)
            {
                ReadNode32(reader).ToArray();
            } else
            {
                ReadNode64(reader).ToArray();
            }
            var footer = new FbxFooter();
            Debug.Assert(reader.ReadBytes(FbxFooter.IdSignature.Length).Equal(FbxFooter.IdSignature));
            Debug.Assert(IsZeroBytes(reader, 4));
            var pos = reader.Position;
            var alignmentPaddingSize = ((pos + 15) & ~15) - pos;
            if (alignmentPaddingSize == 0)
            {
                alignmentPaddingSize = 16;
            }
            Debug.Assert(IsZeroBytes(reader, (int)alignmentPaddingSize));
            footer.Version = reader.ReadUInt32();
            Debug.Assert(footer.Version == header.Version);
            Debug.Assert(IsZeroBytes(reader, 120));
            Debug.Assert(reader.ReadBytes(FbxFooter.Signature.Length).Equal(FbxFooter.Signature));
            return null;
        }

        private bool IsZeroBytes(IBundleBinaryReader reader, int count)
        {
            var buffer = reader.ReadBytes(count);
            return !buffer.Where(x => x > 0).Any();
        }

        private IEnumerable<NodeRecord> ReadNode64(IBundleBinaryReader reader)
        {
            var node = new NodeRecord();
            node.EndOffset = reader.ReadUInt64();
            node.PropertyCount = reader.ReadUInt64();
            node.PropertyListCount = reader.ReadUInt64();
            node.NameLength = reader.ReadByte();
            if (node.EndOffset == 0 && node.PropertyCount == 0 && node.PropertyListCount == 0 && node.NameLength == 0)
            {
                yield break;
            }
            node.Name = Encoding.UTF8.GetString(reader.ReadBytes(node.NameLength));
            var pos = reader.Position;
            node.PropertyItems = reader.ReadArray((int)node.PropertyCount, (_, _) => ReadProperty(reader));
            Debug.Assert(reader.Position == pos + (long)node.PropertyListCount);
            var items = new List<NodeRecord>();
            foreach (var item in ReadNode64(reader))
            {
                items.Add(item);
                if (reader.Position >= (long)node.EndOffset)
                {
                    break;
                }
            }
            node.NestedItems = [.. items];
            yield return node;
        }
        private IEnumerable<NodeRecord> ReadNode32(IBundleBinaryReader reader)
        {
            var node = new NodeRecord();
            node.EndOffset = reader.ReadUInt32();
            node.PropertyCount = reader.ReadUInt32();
            node.PropertyListCount = reader.ReadUInt32();
            node.NameLength = reader.ReadByte();
            if (node.EndOffset == 0 && node.PropertyCount == 0 && node.PropertyListCount == 0 && node.NameLength == 0)
            {
                yield break;
            }
            node.Name = Encoding.UTF8.GetString(reader.ReadBytes(node.NameLength));
            var pos = reader.Position;
            node.PropertyItems = reader.ReadArray((int)node.PropertyCount, (_, _) => ReadProperty(reader));
            Debug.Assert(reader.Position == pos + (long)node.PropertyListCount);
            var items = new List<NodeRecord>();
            foreach (var item in ReadNode32(reader))
            {
                items.Add(item);
                if (reader.Position >= (long)node.EndOffset)
                {
                    break;
                }
            }
            node.NestedItems = [.. items];
            yield return node;
        }

        private PropertyRecord ReadProperty(IBundleBinaryReader reader)
        {
            var data = new PropertyRecord();
            data.TypeCode = (PropertyTypeCode)reader.ReadByte();
            switch (data.TypeCode)
            {
                case PropertyTypeCode.BYTE:
                    reader.ReadByte();
                    break;
                case PropertyTypeCode.SHORT:
                    reader.ReadInt16();
                    break;
                case PropertyTypeCode.BOOL:
                    reader.ReadBoolean();
                    break;
                case PropertyTypeCode.CHAR:
                    reader.ReadByte();
                    break;
                case PropertyTypeCode.INT:
                    reader.ReadInt32();
                    break;
                case PropertyTypeCode.FLOAT:
                    reader.ReadSingle();
                    break;
                case PropertyTypeCode.DOUBLE:
                    reader.ReadDouble();
                    break;
                case PropertyTypeCode.LONG:
                    reader.ReadInt64();
                    break;
                case PropertyTypeCode.BINARY:
                    {
                        var len = reader.ReadUInt32();
                        reader.ReadBytes((int)len);
                        break;
                    }
                case PropertyTypeCode.STRING:
                    {
                        var len = reader.ReadUInt32();
                        Encoding.UTF8.GetString(reader.ReadBytes((int)len));
                        break;
                    }
                case PropertyTypeCode.ARRAY_BOOL:
                    ReadArray(reader, r => r.ReadBoolean());
                    break;
                case PropertyTypeCode.ARRAY_UBYTE:
                    ReadArray(reader, r => r.ReadByte());
                    break;
                case PropertyTypeCode.ARRAY_INT:
                    ReadArray(reader, r => r.ReadInt32());
                    break;
                case PropertyTypeCode.ARRAY_LONG:
                    ReadArray(reader, r => r.ReadInt64());
                    break;
                case PropertyTypeCode.ARRAY_FLOAT:
                    ReadArray(reader, r => r.ReadSingle());
                    break;
                case PropertyTypeCode.ARRAY_DOUBLE:
                    ReadArray(reader, r => r.ReadDouble());
                    break;
                default:
                    break;
            }
            return data;
        }

        private T[] ReadArray<T>(IBundleBinaryReader reader, Func<IBundleBinaryReader, T> cb)
        {
            var count = reader.ReadUInt32();
            var encoding = reader.ReadUInt32();
            var compressedLength = reader.ReadUInt32();
            Debug.Assert(encoding < 2);
            if (encoding == 0)
            {
                return reader.ReadArray((int)count, (_,_) => cb(reader));
            }
            var r = new BundleBinaryReader(new ZLibStream(new PartialStream(reader.BaseStream, compressedLength), CompressionMode.Decompress), EndianType.LittleEndian);
            return r.ReadArray((int)count, (_, _) => cb(r));
        }
    }
}
