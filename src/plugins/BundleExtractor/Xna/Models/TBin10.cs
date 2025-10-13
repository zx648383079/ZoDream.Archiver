namespace ZoDream.BundleExtractor.Xna.Models
{
    internal class TBin10
    {
        public string Format { get; internal set; }
        public string Id { get; internal set; }
        public string Description { get; internal set; }
        public Property?[] Properties { get; internal set; }
        public TileSheet?[] TileSheets { get; internal set; }
        public Layer?[] Layers { get; internal set; }
    }
}
