namespace ZoDream.BundleExtractor.Compression
{
    public readonly struct CrunchPalette(uint ofs, uint size, uint num)
    {
        public uint Offset => ofs;
        public uint Size => size;

        public uint Count => num;
    }
}
