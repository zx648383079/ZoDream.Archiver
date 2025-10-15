using System.Collections.Generic;
using System.Linq;
using UnityEngine.Document;

namespace ZoDream.SourceGenerator
{
    public class TypeNodeComparer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns>返回 source </returns>
        public VirtualDocument Compare(VirtualDocument source, VirtualDocument target)
        {
            var res = new VirtualDocument()
            {
                Version = source.Version,
            };
            var diffItems = new List<VirtualNode>();
            var targetMaps = new Dictionary<string, VirtualNode>();
            foreach (var item in DeepEach(target))
            {
                targetMaps.TryAdd(item.Type, item);
                targetMaps.TryAdd($"{item.Type}{item.Name}", item);
            }
            foreach (var item in DeepEach(source))
            {
                if ((targetMaps.TryGetValue(item.Type, out VirtualNode? node) || targetMaps.TryGetValue($"{item.Type}{item.Name}", out node)) && Compare(item, node))
                {
                    continue;
                }
                diffItems.Add(item);
            }
            res.Children = diffItems.ToArray();
            return res;
        }

        private static bool Compare(VirtualNode source, VirtualNode target)
        {
            if (source.Type != target.Type)
            {
                return false;
            }
            if (source.Name == target.Name && IsAlign(source) != IsAlign(target))
            {
                return false;
            }
            if (source.Type is "string" or "TypelessData")
            {
                return true;
            }
            if (source.Children?.Length != target.Children?.Length)
            {
                return false;
            }
            if (source.Type is not "map" and not "vector" and not "Array" and not "pair" and not "set")
            {
                return true;
            }
            if (source.Children?.Length > 0 && target.Children?.Length > 0)
            {
                for (int i = 0; i < source.Children.Length; i++)
                {
                    if (!Compare(source.Children[i], target.Children[i]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static bool IsAlign(VirtualNode node)
        {
            return node.MetaFlag.HasFlag(TransferMetaFlags.AlignBytes);
        }

        private static IEnumerable<VirtualNode> DeepEach(IEnumerable<VirtualNode> items)
        {
            foreach (var item in items)
            {
                if (item.Type is "string" or "TypelessData" 
                    or "bool" or "double" 
                    or "float" or "UInt64" or  "unsigned long long" or "FileSize" or "long long" 
                    or "SInt64" or "UInt32" or  "unsigned int" or "Type*" or "int" or "SInt32" 
                    or "UInt16" or "unsigned short" or "short" or "SInt16" or "UInt8" or "char" or "SInt8")
                {
                    continue;
                }
                if (item.Type is not "map" and not "vector" and not "Array" and not "pair" and not "set")
                {
                    yield return item;
                }
                if (item.Children?.Length > 0)
                {
                    foreach (var it in DeepEach(item.Children))
                    {
                        yield return it;
                    }
                }
            }
        }
    }
}
