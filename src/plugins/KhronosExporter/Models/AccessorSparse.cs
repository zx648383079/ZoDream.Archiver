using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.KhronosExporter.Models
{
    public class AccessorSparse : ExtraProperties
    {
        public int Count { get; set; }

        public AccessorSparseIndices Indices { get; set; }

        public AccessorSparseValues Values { get; set; }
    }
}
