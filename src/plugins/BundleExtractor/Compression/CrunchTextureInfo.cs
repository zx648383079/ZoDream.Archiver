namespace ZoDream.BundleExtractor.Compression
{
    internal class CrunchTextureInfo
    {
        public CrunchHeader Header { get; set; }

        public uint Width => Header.Width;
        public uint Height => Header.Height;

        public int BytesPerBlock => Header.Format is CrunchFormat.Dxt1 
            or CrunchFormat.Dxt5a or CrunchFormat.Etc1 or CrunchFormat.Etc2 or CrunchFormat.Etc1s ? 8 : 16;

    }
}
