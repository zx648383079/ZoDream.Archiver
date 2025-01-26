using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.KhronosExporter.Models
{
    public class AnimationChannel : ExtraProperties
    {

        public int Sampler {  get; set; }

        public AnimationChannelTarget Target { get; set; }


    }
}
