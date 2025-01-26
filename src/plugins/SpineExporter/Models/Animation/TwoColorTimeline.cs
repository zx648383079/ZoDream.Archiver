using SkiaSharp;

namespace ZoDream.SpineExporter.Models
{
    public class TwoColorTimeline: CurveTimeline
    {
        public TwoColorTimeline(int frameCount): base(frameCount)
        {
            Frames = new float[frameCount];
            ColorFrames = new SKColor[frameCount];
            Color2Frames = new SKColor[frameCount];
        }
        public int SlotIndex { get; set; }
        public float[] Frames { get; set; }// time, r, g, b, a, r2, g2, b2, ...

        public SKColor[] ColorFrames { get; set; }
        public SKColor[] Color2Frames { get; set; }

        public override int PropertyId => ((int)TimelineType.RGB2 << 24) + SlotIndex;
    }

    public class RGB2Timeline : CurveTimeline
    {
        public RGB2Timeline(int frameCount) : base(frameCount)
        {
            Frames = new float[frameCount];
            ColorFrames = new SKColor[frameCount];
            Color2Frames = new SKColor[frameCount];
        }
        public int SlotIndex { get; set; }
        public float[] Frames { get; set; }// time, r, g, b, a, r2, g2, b2, ...

        public SKColor[] ColorFrames { get; set; }
        public SKColor[] Color2Frames { get; set; }

        public override int PropertyId => ((int)TimelineType.RGB2 << 24) + SlotIndex;
    }
}
