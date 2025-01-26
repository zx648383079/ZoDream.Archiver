using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.KhronosExporter.Models
{
    public class AnimationChannelTarget : ExtraProperties
    {
        public int Node {  get; set; }

        public PropertyPath Path { get; set; }
    }
}
