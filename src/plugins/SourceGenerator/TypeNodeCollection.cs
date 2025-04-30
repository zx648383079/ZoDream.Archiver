using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ZoDream.SourceGenerator
{
    public class TypeNodeCollection(TypeTreeNode[] items) : ICollection<TypeTreeNode>, IEnumerable<TypeTreeNode>
    {
        public TypeNodeCollection()
            : this([])
        {
            
        }
        public string Version { get; set; } = string.Empty;
        public int Count => items.Length;

        public bool IsReadOnly => true;

        public TypeTreeNode this[int index] => items[index];

        public IEnumerator<TypeTreeNode> GetEnumerator()
        {
            foreach (var item in items)
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(TypeTreeNode item)
        {
        }

        public void Clear()
        {
        }

        public bool Contains(TypeTreeNode item)
        {
            return items.Contains(item);
        }

        public void CopyTo(TypeTreeNode[] array, int arrayIndex)
        {
            Array.Copy(items, 0, array, arrayIndex, items.Length);
        }

        public bool Remove(TypeTreeNode item)
        {
            return false;
        }
    }
}
