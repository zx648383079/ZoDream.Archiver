namespace ZoDream.ChmExtractor.Models
{
    internal class ChmItspHeader
    {
        internal const string Signature = "ITSP";
        public int Version { get; set; }               /*  4 */
        public int DirectoryHeaderLength1 { get; set; }            /*  8 */
        public int Unknown_000c { get; set; }          /*  c */
        public uint DirectoryChunkSize { get; set; }              /* 10 */
        public int QuickRefSectionDensity { get; set; }        /* 14 */
        public int IndexTreeDepth { get; set; }           /* 18 */
        public int RootIndexChunkNumber { get; set; }            /* 1c */
        public int FirstPMGLChunkNumber { get; set; }           /* 20 */
        public int LastPMGLChunkNumber { get; set; }          /* 24 */
        public uint NumBlocks { get; set; }            /* 28 */
        public uint DirectoryChunkCount { get; set; }          /* 2c */
        public WindowsLanguageId LangId { get; set; }                /* 30 */
        public byte[] SystemUuid { get; set; } = new byte[16];        /* 34 */
        public int DirectoryHeaderLength2 { get; set; }
        public uint Unknown_0048 { get; set; }
        public uint Unknown_004C { get; set; }
        public uint Unknown_0050 { get; set; }
    }
}
