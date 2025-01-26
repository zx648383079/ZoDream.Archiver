using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.SpineExporter.Models
{
    public class PathConstraintPositionTimeline: CurveTimeline
    {
        public PathConstraintPositionTimeline(int frameCount): base(frameCount)
        {
            Frames = new float[frameCount];
            PositionItems = new float[frameCount];
        }
        public int PathConstraintIndex {  get; set; }
        public float[] Frames { get; set; } // time, position, ...

        public float[] PositionItems { get; set; }

        public override int PropertyId => ((int)TimelineType.PathConstraintPosition << 24) + PathConstraintIndex;
    }
}
