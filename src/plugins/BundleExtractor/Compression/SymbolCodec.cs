using System.IO;
using System.Linq;

namespace ZoDream.BundleExtractor.Compression
{
    public class SymbolCodec
    {
        private static readonly byte[] MOST_PROBABLE_CODELENGTH_CODES = [
                SMALL_ZERO_RUN_CODE,
                LARGE_ZERO_RUN_CODE,
                SMALL_REPEAT_CODE,
                LARGE_REPEAT_CODE,
                0,
                8,
                7,
                9,
                6,
                10,
                5,
                11,
                4,
                12,
                3,
                13,
                2,
                14,
                1,
                15,
                16,
        ];
        const int BIT_BUF_SIZE = 32;
        const byte LARGE_ZERO_RUN_CODE = 18;
        const byte SMALL_REPEAT_CODE = 19;
        const byte LARGE_REPEAT_CODE = 20;
        const byte SMALL_ZERO_RUN_CODE = 17;
        const int MAX_CODELENGTH_CODES = 21;
        const int SMALL_ZERO_RUN_EXTRA_BITS = 3;
        const int LARGE_ZERO_RUN_EXTRA_BITS = 7;
        const int SMALL_NON_ZERO_RUN_EXTRA_BITS = 2;
        const int LARGE_NON_ZERO_RUN_EXTRA_BITS = 6;
        const int MIN_SMALL_ZERO_RUN_SIZE = 3;
        const int MIN_LARGE_ZERO_RUN_SIZE = 11;

        const int SMALL_MIN_NON_ZERO_RUN_SIZE = 3;
        const int LARGE_MIN_NON_ZERO_RUN_SIZE = 7;

        private Stream _input;
        public uint _bitBuf;
        public int _bitCount;

        public bool StartDecoding(Stream input)
        {
            if (input.Length == 0)
            {
                return false;
            }
            _input = input;
            GetBitsInit();
            return true;
        }

        public bool DecodeReceiveStaticDataModel(StaticHuffmanDataModel model)
        {
            uint totalUsedSyms = DecodeBits(TotalBits(DecoderTables.MAX_SUPPORTED_SYMS));

            if (totalUsedSyms == 0)
            {
                model.Clear();
            }

            model.Resize((int)totalUsedSyms);

            uint numCodelengthCodesToSend = DecodeBits(5);

            if (numCodelengthCodesToSend < 1 || numCodelengthCodesToSend > MAX_CODELENGTH_CODES)
            {
                return false;
            }

            var dm = new StaticHuffmanDataModel();
            dm.Resize(MAX_CODELENGTH_CODES);

            foreach (var codeLengthCode in MOST_PROBABLE_CODELENGTH_CODES.Take((int)numCodelengthCodesToSend))
            {
                dm.CodeSizes[codeLengthCode] = (byte)DecodeBits(3);
            }

            if (!dm.PrepareDecoderTables())
            {
                return false;
            }

            uint ofs = 0;
            while (ofs < totalUsedSyms)
            {
                uint numRemaining = totalUsedSyms - ofs;

                uint code = Decode(dm);

                if (code <= 16)
                {
                    model.CodeSizes[ofs] = (byte)code;
                    ofs++;
                }
                else if (code == SMALL_ZERO_RUN_CODE)
                {
                    uint len = DecodeBits(SMALL_ZERO_RUN_EXTRA_BITS) + MIN_SMALL_ZERO_RUN_SIZE;

                    if (len > numRemaining)
                    {
                        return false;
                    }

                    ofs += len;
                }
                else if (code == LARGE_ZERO_RUN_CODE)
                {
                    uint len = DecodeBits(LARGE_ZERO_RUN_EXTRA_BITS) + MIN_LARGE_ZERO_RUN_SIZE;

                    if (len > numRemaining)
                    {
                        return false;
                    }

                    ofs += len;
                }
                else if (code == SMALL_REPEAT_CODE || code == LARGE_REPEAT_CODE)
                {
                    uint len;

                    if (code == SMALL_REPEAT_CODE)
                    {
                        len = DecodeBits(SMALL_NON_ZERO_RUN_EXTRA_BITS) + SMALL_MIN_NON_ZERO_RUN_SIZE;
                    }
                    else
                    {
                        len = DecodeBits(LARGE_NON_ZERO_RUN_EXTRA_BITS) + LARGE_MIN_NON_ZERO_RUN_SIZE;
                    }

                    if (ofs == 0 || len > numRemaining)
                    {
                        return false;
                    }

                    uint prev = model.CodeSizes[ofs - 1];
                    if (prev == 0)
                    {
                        return false;
                    }

                    uint end = ofs + len;
                    while (ofs < end)
                    {
                        model.CodeSizes[ofs] = (byte)prev;
                        ofs++;
                    }
                }
                else
                {
                    return false;
                }
            }
            return model.PrepareDecoderTables();
        }

        public uint DecodeBits(uint numBits)
        {
            if (numBits == 0)
            {
                return 0;
            }
            if (numBits > 16)
            {
                uint a = GetBits(numBits - 16);
                uint b = GetBits(16);
                return (a << 16) | b;
            }
            return GetBits(numBits);
        }

        public uint Decode(StaticHuffmanDataModel model)
        {
            var pTables = model.DecodeTables;
            if (this._bitCount < 24)
            {
                if (this._bitCount < 16)
                {
                    uint c0 = 0;
                    uint c1 = 0;
                    if (_input.Position < _input.Length)
                    {
                        c0 = (uint)_input.ReadByte();
                    }
                    if (_input.Position < _input.Length)
                    {
                        c1 = (uint)_input.ReadByte();
                    }
                    _bitCount += 16;
                    uint c = (c0 << 8) | c1;
                    _bitBuf |= c << (32 - _bitCount);
                }
                else
                {
                    uint c;
                    if (_input.Position < _input.Length)
                    {
                        c = (uint)_input.ReadByte();
                    }
                    else
                    {
                        c = 0;
                    }
                    _bitCount += 8;
                    _bitBuf |= c << (32 - _bitCount);
                }
            }
            uint k = (_bitBuf >> 16) + 1;
            uint sym;
            uint len;
            if (k <= pTables.TableMaxCode)
            {
                uint t = pTables.Lookup[(int)(_bitBuf >> (int)(32 - pTables.TableBits))];
                if (t == uint.MaxValue)
                {
                    return 0;
                }
                sym = t & (ushort.MaxValue);
                len = t >> 16;
                if (model.CodeSizes[sym] != len)
                {
                    return 0;
                }
            }
            else
            {
                len = pTables.DecodeStartCodeSize;
                while (true)
                {
                    if (k <= pTables.MaxCodes[len - 1])
                    {
                        break;
                    }
                    len++;
                }
                int valPtr = pTables.ValPtrs[len - 1] + (int)(_bitBuf >> (int)(32 - len));
                if ((uint)valPtr >= model.TotalSyms)
                {
                    return 0;
                }
                sym = pTables.SortedSymbolOrder[valPtr];
            }
            _bitBuf <<= (int)len;
            _bitCount -= (int)len;
            return sym;
        }

        public void StopDecoding() 
        {
        }

        private void GetBitsInit()
        {
            _bitBuf = 0;
            _bitCount = 0;
        }

        private uint GetBits(uint numBits)
        {
            if (numBits > 32)
            {
                return 0;
            }
            while (_bitCount < (int)numBits)
            {
                uint c = 0;
                if (_input.Position < _input.Length)
                {
                    c = (uint)_input.ReadByte();
                }
                _bitCount += 8;
                if (_bitCount > BIT_BUF_SIZE)
                {
                    return 0;
                }
                _bitBuf |= c << (BIT_BUF_SIZE - _bitCount);
            }
            uint result = _bitBuf >> (BIT_BUF_SIZE - (int)numBits);
            _bitBuf <<= (int)numBits;
            _bitCount -= (int)numBits;
            return result;
        }

        internal static uint TotalBits(uint v)
        {
            uint l = 0;
            while (v > 0)
            {
                v >>= 1;
                l += 1;
            }
            return l;
        }
    }
}
