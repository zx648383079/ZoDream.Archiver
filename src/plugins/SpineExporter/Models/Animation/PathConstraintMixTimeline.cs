using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.SpineExporter.Models
{
    public class PathConstraintMixTimeline: CurveTimeline
    {
        public PathConstraintMixTimeline(int frameCount): base(frameCount)
        {
            Frames = new float[frameCount];
            RotateItems = new float[frameCount];
            TranslateItems = new float[frameCount];
        }
        public int PathConstraintIndex {  get; set; }
        public float[] Frames { get; set; }// time, rotate mix, translate mix, ...

        public float[] RotateItems { get; set; }
        public float[] TranslateItems { get; set; }

        public override int PropertyId => ((int)TimelineType.PathConstraintMix << 24) + PathConstraintIndex;
    }
}
