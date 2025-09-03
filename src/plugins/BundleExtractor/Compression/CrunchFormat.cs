namespace ZoDream.BundleExtractor.Compression
{
    public enum CrunchFormat : byte
    {
        Dxt1 = 0,

        Dxt3,

        CCrnfmtDxt5,

        // Various DXT5 derivatives
        Dxt5CcxY, // Luma-chroma
        Dxt5XGxR, // Swizzled 2-component
        Dxt5XGbr, // Swizzled 3-component
        Dxt5Agbr, // Swizzled 4-component

        // ATI 3DC and X360 DXN
        DxnXy,
        DxnYx,

        // DXT5 alpha blocks only
        Dxt5a,

        Etc1,
        Etc2,
        Etc2a,
        Etc1s,
        Etc2as,

        Total,
    }
}
