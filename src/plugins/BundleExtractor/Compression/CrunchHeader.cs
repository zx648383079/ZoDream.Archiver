namespace ZoDream.BundleExtractor.Compression
{
    public class CrunchHeader
    {
        internal const int MIN_SIZE = 74;
        internal static byte[] Signature = "Hx"u8.ToArray();
      

        public ushort HeaderSize { get; internal set; }
        public ushort HeaderCrc { get; internal set; }
        public uint DataSize { get; internal set; }
        public ushort DataCrc { get; internal set; }
        public ushort Width { get; internal set; }
        public ushort Height { get; internal set; }
        public byte Faces { get; internal set; }
        public CrunchFormat Format { get; internal set; }
        public ushort Flags { get; internal set; }
        public uint Reserved { get; internal set; }
        public uint Userdata0 { get; internal set; }
        public uint Userdata1 { get; internal set; }
        public CrunchPalette ColorEndpoints { get; internal set; }
        public CrunchPalette ColorSelectors { get; internal set; }
        public CrunchPalette AlphaEndpoints { get; internal set; }
        public CrunchPalette AlphaSelectors { get; internal set; }
        public ushort TablesSize { get; internal set; }
        public uint TablesOffset { get; internal set; }
        public uint[] LevelOffsets { get; internal set; }
    }
}
