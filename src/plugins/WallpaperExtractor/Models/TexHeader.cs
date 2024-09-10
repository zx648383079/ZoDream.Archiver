using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.WallpaperExtractor.Models
{
    internal class TexHeader
    {
        public TexFormat Format { get; set; }
        public TexFlags Flags { get; set; }
        public int TextureWidth { get; set; }
        public int TextureHeight { get; set; }
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
        public uint UnkInt0 { get; set; }

        public bool IsGift => (Flags & TexFlags.IsGif) == TexFlags.IsGif;

    }
}
