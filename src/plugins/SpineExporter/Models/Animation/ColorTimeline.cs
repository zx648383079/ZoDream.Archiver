using SkiaSharp;

namespace ZoDream.SpineExporter.Models
{
    public class ColorTimeline: CurveTimeline
    {
        public ColorTimeline(int frameCount): base(frameCount)
        {
            Frames = new float[frameCount];
            ColorFrames = new SKColor[frameCount];
        }
        public int SlotIndex {  get; set; }
        public float[] Frames { get; set; }
        public SKColor[] ColorFrames { get; set; }

        public override int PropertyId => ((int)TimelineType.RGB << 24) + SlotIndex;
    }

    public class RGBTimeline : CurveTimeline
    {
        public RGBTimeline(int frameCount) : base(frameCount)
        {
            Frames = new float[frameCount];
            ColorFrames = new SKColor[frameCount];
        }
        public int SlotIndex { get; set; }
        public float[] Frames { get; set; }
        public SKColor[] ColorFrames { get; set; }

        public override int PropertyId => ((int)TimelineType.RGB << 24) + SlotIndex;
    }

    public class AlphaTimeline : CurveTimeline
    {
        public AlphaTimeline(int frameCount) : base(frameCount)
        {
            Frames = new float[frameCount];
            ColorFrames = new SKColor[frameCount];
        }
        public int SlotIndex { get; set; }
        public float[] Frames { get; set; }
        public SKColor[] ColorFrames { get; set; }

        public override int PropertyId => ((int)TimelineType.Alpha << 24) + SlotIndex;
    }
}
