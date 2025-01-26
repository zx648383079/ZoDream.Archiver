namespace ZoDream.SpineExporter.Models
{
    public class AtlasRegion
    {
        public string Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public float U { get; set; }
        public float V { get; set; }
        public float U2 { get; set; }
        public float V2 { get; set; }
        public float OffsetX { get; set; }
        public float OffsetY { get; set; }
        public int OriginalWidth { get; set; }
        public int OriginalHeight { get; set; }
        public int Index { get; set; }
        public int Rotate { get; set; }
        public int[] Splits { get; set; }
        public int[] Pads { get; set; }
    }
}
