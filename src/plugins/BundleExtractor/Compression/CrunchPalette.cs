namespace ZoDream.BundleExtractor.Compression
{
    internal readonly struct CrunchPalette(uint ofs, uint size, uint num)
    {
        public uint Offset => ofs;
        public uint Size => size;

        public uint Count => num;
    }
}
