using ZoDream.Shared.Numerics;

namespace ZoDream.BundleExtractor.Xna.Models
{
    internal class TileSheet
    {
        public string Id { get; internal set; }
        public string Description { get; internal set; }
        public string Image { get; internal set; }
        public Vector2Int SheetSize { get; internal set; }
        public Vector2Int TileSize { get; internal set; }
        public Vector2Int Margin { get; internal set; }
        public Vector2Int Spacing { get; internal set; }
        public Property?[] Properties { get; internal set; }
    }
}
