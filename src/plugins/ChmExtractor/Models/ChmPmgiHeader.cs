namespace ZoDream.ChmExtractor.Models
{
    internal class ChmPmgiHeader
    {
        internal const string Signature = "PMGI";
        public uint FreeSpaceLength { get; set; }            /*  4 */
    }
}
