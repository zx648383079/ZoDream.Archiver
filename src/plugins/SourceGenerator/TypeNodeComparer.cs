using System.Collections.Generic;
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
            // TODO 获取父级
            //foreach (var item in DeepEach(source))
            //{
            //    foreach (var child in diffItems)
            //    {
            //        if (Contains(item, child))
            //        {
            //            diffItems.Add(child);
            //        }
            //    }
            //}
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

        private static bool Contains(VirtualNode parent, VirtualNode child)
        {
            if (parent.Type == child.Type)
            {
                return false;
            }
            if (parent.Children is null || parent.Children.Length == 0)
            {
                return false;
            }
            foreach (var item in parent.Children)
            {
                if (item.Type == child.Type)
                {
                    return true;
                }
                foreach (var it in GetNodeType(item))
                {
                    if (it == child.Type)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static IEnumerable<VirtualNode> DeepEach(IEnumerable<VirtualNode> items)
        {
            foreach (var item in items)
            {
                if (IsValueType(item))
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

        private static IEnumerable<string> GetNodeType(VirtualNode node)
        {
            if (node.Type is "Array" or "map" or "set" or "vector")
            {
                foreach (var item in GetNodeType(node.Children[1]))
                {
                    yield return item;
                }
            }
            else if (node.Type is "pair")
            {
                foreach(var item in node.Children)
                {
                    yield return item.Type;
                }
            } else
            {
                yield return node.Type;
            }
        }

        private static bool IsValueType(VirtualNode node)
        {
            return node.Type is "string" or "TypelessData"
                    or "bool" or "double"
                    or "float" or "UInt64" or "unsigned long long" or "FileSize" or "long long"
                    or "SInt64" or "UInt32" or "unsigned int" or "Type*" or "int" or "SInt32"
                    or "UInt16" or "unsigned short" or "short" or "SInt16" or "UInt8" or "char" or "SInt8"
        }
    }
}
