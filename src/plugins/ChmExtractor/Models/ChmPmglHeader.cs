namespace ZoDream.ChmExtractor.Models
{
    internal class ChmPmglHeader
    {
        internal const string Signature = "PMGL";
        public uint FreeSpaceLength { get; set; }            /*  4 */
        public uint Unknown_0008 { get; set; }          /*  8 */
        public int PrevChunkNumber { get; set; }           /*  c */
        public int NextChunkNumber { get; set; }           /* 10 */
    }
}
