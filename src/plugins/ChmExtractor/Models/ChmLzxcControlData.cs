namespace ZoDream.ChmExtractor.Models
{
    internal class ChmLzxcControlData
    {
        internal const string Signature = "LZXC";

        public uint Size { get; set; }                  /*  0        */
        // signature
        public uint Version { get; set; }                /*  8        */
        public uint ResetInterval { get; set; }          /*  c        */
        public uint WindowSize { get; set; }            /* 10        */
        public uint WindowsPerReset { get; set; }        /* 14        */
        public uint Unknown_18 { get; set; }             /* 18        */
    }
}
