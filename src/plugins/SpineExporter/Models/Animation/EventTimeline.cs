namespace ZoDream.SpineExporter.Models
{
    public class EventTimeline: Timeline
    {
        public EventTimeline(int frameCount)
        {
            Frames = new float[frameCount];
            Events = new Event[frameCount];
        }
        public float[] Frames { get; set; } // time, ...
        public Event[] Events { get; set; }
        public int FrameCount { get; set; }

        public int PropertyId => ((int)TimelineType.Event << 24);
    }
}
