namespace ZoDream.BundleExtractor.Xna.Models
{
    internal class StaticTile : BaseTile
    {
        public string TileSheet { get; internal set; }
        public int TileIndex { get; internal set; }
        public byte BlendMode { get; internal set; }
    }
}
