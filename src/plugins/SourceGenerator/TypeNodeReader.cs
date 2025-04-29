using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using UnityEngine;

namespace ZoDream.SourceGenerator
{
    public class TypeNodeReader(Stream input)
    {

        public TypeTreeNode[] Read()
        {
            var doc = JsonDocument.Parse(input);
            if (doc == null)
            {
                return [];
            }
            var root = doc.RootElement;
            var res = new List<TypeTreeNode>();
            if (root.TryGetProperty("Classes", out var items))
            {
                foreach (var item in items.EnumerateArray())
                {
                    if (item.TryGetProperty("EditorRootNode", out var node))
                    {
                        var n = Read(node);
                        if (n != null)
                        {
                            res.Add(n);
                        }
                    }
                }
            }
            return [..res];
        }

        private TypeTreeNode? Read(JsonElement node)
        {
            if (node.ValueKind != JsonValueKind.Object)
            {
                return null;
            }
            var res = new TypeTreeNode();
            foreach (var item in node.EnumerateObject())
            {
                switch (item.Name)
                {
                    case "TypeName":
                        res.Type = item.Value.GetString() ?? string.Empty;
                        break;
                    case "Name":
                        res.Name = item.Value.GetString() ?? string.Empty;
                        break;
                    case "Level":
                        res.Level = item.Value.GetByte();
                        break;
                    case "ByteSize":
                        res.ByteSize = item.Value.GetInt32();
                        break;
                    case "Index":
                        res.Index = item.Value.GetInt32();
                        break;
                    case "Version":
                        res.Version = item.Value.GetInt32();
                        break;
                    case "TypeFlags":
                        res.TypeFlags = item.Value.GetInt32();
                        break;
                    case "MetaFlag":
                        res.MetaFlag = (TransferMetaFlags)item.Value.GetInt32();
                        break;
                    case "SubNodes":
                        res.Children = ReadArray(item.Value);
                        break;
                    default:
                        break;
                }
            }
            return res;
        }

        private TypeTreeNode[] ReadArray(JsonElement items)
        {
            if (items.ValueKind != JsonValueKind.Array)
            {
                return [];
            }
            var res = new List<TypeTreeNode>();
            foreach (var item in items.EnumerateArray())
            {
                var n = Read(item);
                if (n != null)
                {
                    res.Add(n);
                }
            }
            return [.. res];
        }
    }
}
