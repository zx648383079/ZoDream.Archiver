namespace ZoDream.SpineExporter.Models
{
    public class RotateTimeline: CurveTimeline
    {
        public RotateTimeline(int frameCount): base(frameCount)
        {
            Frames = new float[frameCount];
            AngleItems = new float[frameCount];
        }
        public int BoneIndex {  get; set; }
        public float[] Frames { get; set; }

        public float[] AngleItems { get; set; }

        public override int PropertyId => ((int)TimelineType.Rotate << 24) + BoneIndex;
    }
}
