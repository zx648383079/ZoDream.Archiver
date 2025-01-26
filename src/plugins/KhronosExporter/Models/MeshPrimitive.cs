using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.KhronosExporter.Models
{
    public class MeshPrimitive : ExtraProperties
    {
        public Dictionary<string, int> Attributes { get; set; }
        public int Indices { get; set; }

        public int Material {  get; set; }

        public PrimitiveType Mode { get; set; }

        public Dictionary<string, int>[] Targets { get; set; }
    }
}
