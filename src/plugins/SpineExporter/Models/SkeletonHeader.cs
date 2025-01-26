namespace ZoDream.SpineExporter.Models
{
    public class SkeletonHeader
    {
        public string Hash { get; set; }

        public float Height { get; set; }

        public float Width { get; set; }

        public string ImagesPath { get; set; }

        public string Version { get; set; }
        public float Fps { get; set; }
        public float X { get; internal set; }
        public float Y { get; internal set; }
        public float ReferenceScale { get; internal set; }
        public string AudioPath { get; internal set; }
    }
}
