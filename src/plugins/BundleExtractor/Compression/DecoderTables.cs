using System;
using System.Collections.Generic;
using System.Linq;

namespace ZoDream.BundleExtractor.Compression
{
    public class DecoderTables
    {
        internal const int MAX_EXPECTED_CODE_SIZE = 16;
        internal const int MAX_TABLE_BITS = 11;
        internal const int MAX_SUPPORTED_SYMS = 8192;

        public uint NumSyms;
        public uint TotalUsedSyms;
        public uint TableBits;
        public uint TableShift;
        public uint TableMaxCode;
        public uint DecodeStartCodeSize;

        public byte MinCodeSize;
        public byte MaxCodeSize;

        public uint[] MaxCodes = new uint[MAX_EXPECTED_CODE_SIZE + 1];
        public int[] ValPtrs = new int[MAX_EXPECTED_CODE_SIZE + 1];

        public uint CurLookupSize;
        public List<uint> Lookup = [];

        public uint CurSortedSymbolOrderSize;
        public List<ushort> SortedSymbolOrder = [];


        public bool Init(uint num_syms, byte[] p_codesizes, uint table_bits)
        {
            uint[] min_codes = new uint[MAX_EXPECTED_CODE_SIZE];

            if (num_syms == 0 || table_bits > MAX_TABLE_BITS)
            {
                return false;
            }
            NumSyms = num_syms;
            uint[] num_codes = new uint[MAX_EXPECTED_CODE_SIZE + 1];

            foreach (var c in p_codesizes.Take((int)num_syms))
            {
                if (c != 0 && c < num_codes.Length)
                {
                    num_codes[c]++;
                }
            }

            uint[] sorted_positions = new uint[MAX_EXPECTED_CODE_SIZE + 1];
            uint cur_code = 0;
            uint total_used_syms = 0;
            uint max_code_size = 0;
            uint min_code_size = uint.MaxValue;

            for (int i = 1; i <= MAX_EXPECTED_CODE_SIZE; i++)
            {
                uint n = num_codes[i];

                if (n == 0)
                {
                    MaxCodes[i - 1] = 0;
                }
                else
                {
                    min_code_size = Math.Min(min_code_size, (uint)i);
                    max_code_size = Math.Max(max_code_size, (uint)i);

                    min_codes[i - 1] = cur_code;

                    MaxCodes[i - 1] = cur_code + n - 1;
                    MaxCodes[i - 1] = 1 + ((MaxCodes[i - 1] << (16 - i)) | (uint)((1 << (16 - i)) - 1));
                    ValPtrs[i - 1] = (int)total_used_syms;
                    sorted_positions[i] = total_used_syms;

                    cur_code += n;
                    total_used_syms += n;
                }
                cur_code <<= 1;
            }

            TotalUsedSyms = total_used_syms;
            if (total_used_syms > CurSortedSymbolOrderSize)
            {
                CurSortedSymbolOrderSize = total_used_syms;

                if (!IsPowerOfTwo(total_used_syms))
                {
                    CurSortedSymbolOrderSize = Math.Min(num_syms, NextPowerOfTwo(total_used_syms));
                }

                SortedSymbolOrder = new List<ushort>((int)CurSortedSymbolOrderSize);
            }

            min_code_size = (byte)min_code_size;
            max_code_size = (byte)max_code_size;

            for (int i = 0; i < num_syms; i++)
            {
                uint c = p_codesizes[i];

                if (c != 0 && c < num_codes.Length)
                {
                    int code_index = (int)c;

                    if (num_codes[code_index] == 0)
                    {
                        return false;
                    }

                    uint sorted_pos = sorted_positions[code_index];
                    sorted_positions[code_index]++;

                    if (sorted_pos >= total_used_syms)
                    {
                        return false;
                    }

                    SortedSymbolOrder[(int)sorted_pos] = (ushort)i;
                }
            }

            if (table_bits <= min_code_size)
            {
                table_bits = 0;
            }

            TableBits = table_bits;
            if (table_bits != 0)
            {
                uint table_size = 1u << (int)table_bits;

                if (table_size > CurLookupSize)
                {
                    CurLookupSize = table_size;
                    Lookup = new List<uint>((int)table_size);
                }

                for (uint codesize = 1; codesize <= table_bits; codesize++)
                {
                    if (num_codes[codesize] == 0)
                    {
                        continue;
                    }
                    uint fillsize = table_bits - codesize;
                    uint fillnum = 1u << (int)fillsize;
                    uint min_code = min_codes[codesize - 1];

                    uint max_code = GetUnshiftedMaxCode(codesize);

                    var val_ptr = ValPtrs[codesize - 1];

                    for (uint code = min_code; code <= max_code; code++)
                    {
                        uint sym_index = SortedSymbolOrder[(int)(val_ptr + code - min_code)];

                        if (p_codesizes[sym_index] != codesize)
                        {
                            return false;
                        }

                        for (uint j = 0; j < fillnum; j++)
                        {
                            uint t = j + (code << (int)fillsize);
                            if (t >= (1u << (int)table_bits))
                            {
                                return false;
                            }
                            Lookup[(int)t] = sym_index | (codesize << 16);
                        }
                    }
                }
            }

            for (int i = 0; i < ValPtrs.Length; i++)
            {
                ValPtrs[i] -= (int)min_codes[i];
            }

            TableMaxCode = 0;
            DecodeStartCodeSize = min_code_size;

            if (table_bits != 0)
            {
                uint i = table_bits;

                while (i >= 1)
                {
                    if (num_codes[i] != 0)
                    {
                        TableMaxCode = MaxCodes[i - 1];
                        break;
                    }
                    i--;
                }

                if (i >= 1)
                {
                    DecodeStartCodeSize = table_bits + 1;
                    for (uint j = table_bits + 1; j <= max_code_size; j++)
                    {
                        if (num_codes[j] != 0)
                        {
                            DecodeStartCodeSize = j;
                            break;
                        }
                    }
                }
            }

            if (TableMaxCode == 0)
            {
                TableMaxCode = 0;
            }
            // sentinels
            MaxCodes[MAX_EXPECTED_CODE_SIZE] = uint.MaxValue;
            ValPtrs[MAX_EXPECTED_CODE_SIZE] = 0xFFFFF;

            TableShift = 32 - table_bits;
            return true;
        }

        private uint GetUnshiftedMaxCode(uint len)
        {
            if (!(len >= 1 && len <= MAX_EXPECTED_CODE_SIZE))
            {
                throw new ArgumentOutOfRangeException();
            }
            uint k = MaxCodes[len - 1];
            if (k == 0)
            {
                return uint.MaxValue;
            }
            return (k - 1) >> (16 - (int)len);
        }

        private bool IsPowerOfTwo(uint x)
        {
            return (x & (x - 1)) == 0;
        }

        private uint NextPowerOfTwo(uint x)
        {
            if (x == 0)
            {
                return 1;
            }
            x--;
            x |= x >> 1;
            x |= x >> 2;
            x |= x >> 4;
            x |= x >> 8;
            x |= x >> 16;
            return x + 1;
        }


    }
}
