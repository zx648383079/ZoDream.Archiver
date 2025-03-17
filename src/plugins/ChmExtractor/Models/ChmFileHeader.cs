namespace ZoDream.ChmExtractor.Models
{
    internal class ChmFileHeader
    {
        internal const string Signature = "ITSF";
        public int Version { get; set; }                 /*  4 */
        public int HeaderLen { get; set; }              /*  8 */
        public int Unknown_000c { get; set; }            /*  c */
        public uint LastModified { get; set; }           /* 10 */
        public WindowsLanguageId LangId { get; set; }                 /* 14 */
        public byte[] DirUuid { get; set; } = new byte[16];           /* 18 */
        public byte[] StreamUuid { get; set; } = new byte[16];         /* 28 */
        public ulong UnknownOffset { get; set; }          /* 38 */
        public ulong UnknownLen { get; set; }             /* 40 */
        public ulong DirOffset { get; set; }              /* 48 */
        public ulong DirLen { get; set; }                /* 50 */
        public ulong DataOffset { get; set; }             /* 58 (Not present before V3) */
    }
}
