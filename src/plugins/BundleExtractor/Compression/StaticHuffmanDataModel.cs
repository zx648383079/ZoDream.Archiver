using System;

namespace ZoDream.BundleExtractor.Compression
{
    public class StaticHuffmanDataModel
    {
        public uint TotalSyms;
        public byte[] CodeSizes = [];
        public DecoderTables DecodeTables = new();



        public void Clear()
        {
            DecodeTables = new();
        }

        public uint ComputeDecoderTableBits()
        {
            uint decoderTableBits = 0;
            if (TotalSyms > 16)
            {
                decoderTableBits = Math.Min(1 + CeilLog2i(TotalSyms), DecoderTables.MAX_TABLE_BITS);
            }
            return decoderTableBits;
        }

        public bool PrepareDecoderTables()
        {
            int totalSyms = CodeSizes.Length;
            if (!(totalSyms >= 1 && totalSyms <= DecoderTables.MAX_SUPPORTED_SYMS))
            {
                return false;
            }
            TotalSyms = (uint)totalSyms;
            uint tableBits = ComputeDecoderTableBits();
            return DecodeTables.Init(TotalSyms, CodeSizes, tableBits);
        }

        public uint CeilLog2i(uint v)
        {
            uint l = (uint)Math.Log(v, 2);
            if (l != 32 && v > (1U << (int)l))
            {
                l += 1;
            }
            return l;
        }

        internal void Resize(int codeSizeLength)
        {
            Array.Resize(ref CodeSizes, codeSizeLength);
        }
    }
}
