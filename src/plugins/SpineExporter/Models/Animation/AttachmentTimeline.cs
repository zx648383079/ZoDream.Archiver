namespace ZoDream.SpineExporter.Models
{
    public class AttachmentTimeline: Timeline
    {
        public AttachmentTimeline(int frameCount)
        {
            Frames = new float[frameCount];
            AttachmentNames = new string[frameCount];
        }
        public int SlotIndex {  get; set; }
        public float[] Frames { get; set; }
        public string[] AttachmentNames { get; set; }
        public int FrameCount => Frames.Length;

        public int PropertyId => ((int)TimelineType.Attachment << 24) + SlotIndex;
    }
}
