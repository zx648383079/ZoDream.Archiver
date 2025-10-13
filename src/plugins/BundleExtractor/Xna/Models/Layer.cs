using ZoDream.Shared.Numerics;

namespace ZoDream.BundleExtractor.Xna.Models
{
    internal class Layer
    {
        public string Id { get; internal set; }
        public byte Visible { get; internal set; }
        public string Description { get; internal set; }
        public Vector2Int LayerSize { get; internal set; }
        public Property?[] Properties { get; internal set; }
        public Vector2Int TileSize { get; internal set; }
    }
}
