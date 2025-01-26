using SkiaSharp;

namespace ZoDream.SpineExporter.Models
{
    public class TranslateTimeline: CurveTimeline
    {
        public TranslateTimeline(int frameCount): base(frameCount)
        {
            Frames = new float[frameCount];
            Points = new SKPoint[frameCount];
        }
        public int BoneIndex { get; set; }
        public float[] Frames { get; set; }

        public SKPoint[] Points { get; set; }

        public override int PropertyId => ((int)TimelineType.X << 24) + BoneIndex;
    }
}
