using System.Collections.Generic;

namespace ZoDream.KhronosExporter.Models
{
    public class Mesh : LogicalChildOfRoot
    {
        public IList<MeshPrimitive> Primitives { get; set; } = [];

        public IList<float>? Weights { get; set; }

    }
}
