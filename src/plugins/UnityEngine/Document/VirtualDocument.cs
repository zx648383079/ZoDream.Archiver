using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.Document
{
    public class VirtualDocument : IEnumerable<VirtualNode>
    {

        public VirtualDocument()
        {
            
        }

        public VirtualDocument(VirtualNode[] items)
        {
            Children = items;
        }

        public VirtualDocument(Version version, VirtualNode[] items)
        {
            Version = version;
            Children = items;
        }


        public Version Version { get; set; }
        /// <summary>
        /// 只包含一个 Base 节点及类型
        /// </summary>
        public VirtualNode[] Children { get; set; } = [];

        public int Count => Children.Length;

        public IEnumerator<VirtualNode> GetEnumerator()
        {
            return ((IEnumerable<VirtualNode>)Children).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Children.GetEnumerator();
        }
    }
}
