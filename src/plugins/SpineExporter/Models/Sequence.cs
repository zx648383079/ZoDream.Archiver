namespace ZoDream.SpineExporter.Models
{
    public class Sequence
    {
        public int Start { get; set; }
        public int Digits { get; set; }
        public int SetupIndex { get; set; }
        public TextureRegion[] Regions { get; set; }

        public Sequence(int count)
        {
            Regions = new TextureRegion[count];
        }
    }

    public class TextureRegion
    {
        public float Width { get; set; }
        public float Height { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
    }
}
