using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.KhronosExporter.Models
{
    public class Mesh : LogicalChildOfRoot
    {
        public MeshPrimitive[] Primitives { get; set; }

        public float[] Weights { get; set; }

    }
}
