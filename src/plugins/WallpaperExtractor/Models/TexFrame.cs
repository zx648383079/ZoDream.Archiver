using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.WallpaperExtractor.Models
{
    internal class TexFrame
    {
        public int ImageId { get; set; }
        public float FrameTime { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float WidthY { get; set; }
        public float HeightX { get; set; }
        public float Height { get; set; }
    }
}
