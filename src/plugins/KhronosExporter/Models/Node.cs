using System.Collections.Generic;

namespace ZoDream.KhronosExporter.Models
{
    public class Node : LogicalChildOfRoot
    {
        public int Camera { get; set; } = -1;

        /// <summary>
        /// Nodes
        /// </summary>
        public IList<int> Children { get; set; } = [];

        /// <summary>
        /// Skin
        /// </summary>
        public int Skin { get; set; } = -1;

        public float[] Matrix { get; set; }

        public int Mesh { get; set; } = -1;

        public float[] Rotation { get; set; }

        public float[] Scale { get; set; }

        public float[] Translation { get; set; }

        public float[] Weights { get; set; }

    }
}
