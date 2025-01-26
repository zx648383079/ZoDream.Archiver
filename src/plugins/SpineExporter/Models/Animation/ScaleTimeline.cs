using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.SpineExporter.Models
{
    public class ScaleTimeline: TranslateTimeline
    {
        public ScaleTimeline(int frameCount): base(frameCount)
        {
            
        }
        public override int PropertyId => ((int)TimelineType.ScaleX << 24) + BoneIndex;
    }
}
