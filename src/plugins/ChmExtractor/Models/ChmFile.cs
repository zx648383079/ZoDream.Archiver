using ZoDream.ChmExtractor.Lzx;

namespace ZoDream.ChmExtractor.Models
{
    internal class ChmFile
    {
        public ChmFileHeader Header { get; private set; } = new();

        public ChmItspHeader ItspHeader { get; private set; } = new();
        public long DirOffset => (long)Header.DirOffset + ItspHeader.DirectoryHeaderLength1;
        public long DirLen => (long)Header.DirLen - ItspHeader.DirectoryHeaderLength1;
        public long DataOffset => (long)Header.DataOffset;
        public int IndexRoot => ItspHeader.RootIndexChunkNumber <= -1 ? IndexHead : ItspHeader.RootIndexChunkNumber;
        public int IndexHead => ItspHeader.FirstPMGLChunkNumber;
        public int BlockLen => (int)ItspHeader.DirectoryChunkSize;

        public ulong Span { get; set; }
        public ChmUnitInfo RtUnit { get; private set; } = new();
        public ChmUnitInfo CnUnit { get; private set; } = new();
        public ChmLzxcResetTable ResetTable { get; private set; } = new();

        /* LZX control data */

        public ChmLzxcControlData ControlData { get; private set; } = new();

        public bool CompressionEnabled { get; set; } = true;
        public uint WindowSize  => ControlData.WindowSize;
        public uint ResetInterval => ControlData.ResetInterval;
        public uint ResetBlkCount => WindowSize == 0 ? 0 : ResetInterval * ControlData.WindowsPerReset * 2 / WindowSize;

        /* decompressor state */
        public LzxDecoder? LzxDecoder { get; set; }
        public int LzxLastBlock { get; set; }

        /* cache for decompressed blocks */
        public byte[][] CacheBlocks { get; set; }
        public ulong[] CacheBlockIndices { get; set; }
        public int CacheNumBlocks { get; set; }

    }
}
