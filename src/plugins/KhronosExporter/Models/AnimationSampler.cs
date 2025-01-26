using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.KhronosExporter.Models
{
    public class AnimationSampler : ExtraProperties
    {
        public int Input {  get; set; }

        public AnimationInterpolationMode Interpolation { get; set; }

        public int Output { get; set; }
    }
}
