namespace ZoDream.SpineExporter.Models
{
    public class Event
    {
        public string Name { get; set; }
        public int Int { get; set; }
        public float Float { get; set; }
        public string String { get; set; }
        public string AudioPath { get; internal set; }
        public float Volume { get; internal set; }
        public float Balance { get; internal set; }
    }
}
