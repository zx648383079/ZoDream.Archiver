namespace ZoDream.BundleExtractor.Xna.Models
{
    internal class AnimatedTile : BaseTile
    {
        public int FrameInterval { get; internal set; }
        public StaticTile[] Frames { get; internal set; }
        public char[] Index { get; internal set; }
     
    }
}
