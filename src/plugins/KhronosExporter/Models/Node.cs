using System.Collections.Generic;

namespace ZoDream.KhronosExporter.Models
{
    public class Node : LogicalChildOfRoot
    {
        public int? Camera { get; set; }

        /// <summary>
        /// Nodes
        /// </summary>
        public IList<int>? Children { get; set; }

        /// <summary>
        /// Skin
        /// </summary>
        public int? Skin { get; set; }

        public float[] Matrix { get; set; }

        public int? Mesh { get; set; }

        public float[] Rotation { get; set; }

        public float[] Scale { get; set; }

        public float[] Translation { get; set; }

        public float[] Weights { get; set; }

    }
}
