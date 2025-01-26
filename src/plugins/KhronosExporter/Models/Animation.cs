using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.KhronosExporter.Models
{
    public class Animation : LogicalChildOfRoot
    {
        public AnimationChannel[] Channels {  get; set; }

        public AnimationSampler[] Samplers { get; set; }

    }
}
