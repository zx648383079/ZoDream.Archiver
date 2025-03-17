namespace ZoDream.ChmExtractor.Lzx
{
    internal class LzxState
    {
        public uint R0 { get; set; }         /* for the LRU offset system				*/
        public uint R1 { get; set; }
        public uint R2 { get; set; }
        public ushort MainElements { get; set; }       /* number of main tree elements				*/
        public int HeaderRead { get; set; }     /* have we started decoding at all yet? 	*/
        public LzxConstants.BLOCKTYPE BlockType { get; set; }          /* type of this block						*/
        public uint BlockLength { get; set; }       /* uncompressed length of this block 		*/
        public uint BlockRemaining { get; set; }   /* uncompressed bytes still left to decode	*/
        public uint FramesRead { get; set; }       /* the number of CFDATA blocks processed	*/
        public int IntelFileSize { get; set; }      /* magic header value used for transform	*/
        public int IntelCurPos { get; set; }        /* current offset in transform space		*/
        public int IntelStarted { get; set; }      /* have we seen any translateable data yet?	*/

        public ushort[] PreTreeTable { get; set; }
        public byte[] PreTreeLen { get; set; }
        public ushort[] MainTreeTable { get; set; }
        public byte[] MainTreeLen { get; set; }
        public ushort[] LengthTable { get; set; }
        public byte[] LengthLen { get; set; }
        public ushort[] AlignedTable { get; set; }
        public byte[] AlignedLen { get; set; }

        // NEEDED MEMBERS
        // CAB actualsize
        // CAB window
        // CAB window_size
        // CAB window_posn
        public uint ActualSize { get; set; }
        public byte[] Window { get; set; }
        public uint WindowSize { get; set; }
        public uint WindowPosn { get; set; }
    }
}
