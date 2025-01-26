using SkiaSharp;

namespace ZoDream.SpineExporter.Models
{
    public class RegionAttachment: AttachmentBase
    {
        public const int BLX = 0;
        public const int BLY = 1;
        public const int ULX = 2;
        public const int ULY = 3;
        public const int URX = 4;
        public const int URY = 5;
        public const int BRX = 6;
        public const int BRY = 7;

        public float X { get; set; }
        public float Y { get; set; }
        public float Rotation { get; set; }
        public float ScaleX { get; set; }
        public float ScaleY { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        public SKColor? Color { get; set; }

        public string Path { get; set; }
        public object RendererObject { get; set; }
        public float RegionOffsetX { get; set; }
        public float RegionOffsetY { get; set; }
        public float RegionWidth { get; set; }
        public float RegionHeight { get; set; }
        public float RegionOriginalWidth { get; set; }
        public float RegionOriginalHeight { get; set; }

        public float[] Offset { get; set; }
        public float[] UVs { get; set; }
        public Sequence? Sequence { get; internal set; }
    }
}
