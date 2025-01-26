using System.Collections.Generic;

namespace ZoDream.SpineExporter.Models
{
    public class AtlasPage
    {
        public string Name { get; set; }
        public AtlasFormat Format { get; set; }
        public TextureFilter MinFilter { get; set; }
        public TextureFilter MagFilter { get; set; }
        public TextureWrap UWrap { get; set; } = TextureWrap.ClampToEdge;
        public TextureWrap VWrap { get; set; } = TextureWrap.ClampToEdge;
        public int Width { get; set; }
        public int Height { get; set; }

        public List<AtlasRegion> Items { get; set; } = new();
    }
}
