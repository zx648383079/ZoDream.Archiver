using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.SpineExporter.Models
{
    public class TransformConstraintTimeline: CurveTimeline
    {
        public TransformConstraintTimeline(int frameCount): base(frameCount)
        {
            Frames = new float[frameCount];
            RotateItems = new float[frameCount];
            TranslateItems = new float[frameCount];
            ScaleItems = new float[frameCount];
            ShearItems = new float[frameCount];
        }
        public int TransformConstraintIndex {  get; set; }
        public float[] Frames { get; set; } // time, rotate mix, translate mix, scale mix, shear mix, ...

        public float[] RotateItems { get; set; }
        public float[] TranslateItems { get; set; }
        public float[] ScaleItems { get; set; }
        public float[] ShearItems { get; set; }

        public override int PropertyId => ((int)TimelineType.TransformConstraint << 24) + TransformConstraintIndex;
    }
}
