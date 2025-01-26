using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.KhronosExporter.Models
{
    public class Skin : LogicalChildOfRoot
    {
        public int InverseBindMatrices { get; set; }

        public int Skeleton {  get; set; }
        public int[] Joints { get; set; }

    }
}
