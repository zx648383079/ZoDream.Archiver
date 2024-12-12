using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.AutodeskExporter.Models
{
    internal class NodeRecord
    {
        public ulong EndOffset { get; set; }
        public ulong PropertyCount { get; set; }
        public ulong PropertyListCount { get; set; }
        public byte NameLength { get; set; }
        public string Name { get; set; } = string.Empty;

        public PropertyRecord[] PropertyItems { get; set; }

        public NodeRecord[] NestedItems { get; set; }
    }
}
