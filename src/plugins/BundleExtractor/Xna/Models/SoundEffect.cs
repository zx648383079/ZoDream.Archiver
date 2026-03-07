using System.IO;

namespace ZoDream.BundleExtractor.Xna.Models
{
    internal class SoundEffect
    {
        public byte[] Header { get; internal set; }
        public Stream Data { get; internal set; }
        public int LoopStart { get; internal set; }
        public int LoopLength { get; internal set; }
        public int Duration { get; internal set; }
    }
}
