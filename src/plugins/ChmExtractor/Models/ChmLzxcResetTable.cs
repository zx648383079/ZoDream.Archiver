namespace ZoDream.ChmExtractor.Models
{
    internal class ChmLzxcResetTable
    {
        public uint Version { get; set; }
        public uint BlockCount { get; set; }
        public uint Unknown { get; set; }
        public uint TableOffset { get; set; }
        public ulong UncompressedLen { get; set; }
        public ulong CompressedLen { get; set; }
        public ulong BlockLen { get; set; }
    }
}
