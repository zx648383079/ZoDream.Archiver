using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.KhronosExporter.Models
{
    public class Node : LogicalChildOfRoot
    {
        public int Camera {  get; set; }

        public int[] Children { get; set; }

        public int Skin { get; set; }

        public float[] Matrix { get; set; }

        public int Mesh {  get; set; }

        public float[] Rotation { get; set; }

        public float[] Scale { get; set; }

        public float[] Translation { get; set; }

        public float[] Weights { get; set; }

    }
}
