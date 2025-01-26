namespace ZoDream.SpineExporter.Models
{
    public class IkConstraintTimeline: CurveTimeline
    {
        public IkConstraintTimeline(int frameCount) : base(frameCount)
        {
            Frames = new float[frameCount];
            MixItems = new float[frameCount];
            DirectionItems = new byte[frameCount];
        }
        public int IkConstraintIndex { get; set; }
        public float[] Frames { get; set; } // time, mix, bendDirection, ...

        public float[] MixItems { get; set; }

        public byte[] DirectionItems { get; set; }

        public override int PropertyId => ((int)TimelineType.IkConstraint << 24) + IkConstraintIndex;
    }
}
