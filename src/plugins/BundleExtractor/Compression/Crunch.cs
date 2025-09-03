using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ZoDream.Shared;
using ZoDream.Shared.Drawing;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using NotSupportedException = ZoDream.Shared.NotSupportedException;

namespace ZoDream.BundleExtractor.Compression
{
    public class Crunch(Stream input) : IDisposable
    {
        const int CRNMAX_LEVELS = 16;
        const bool CRND_LITTLE_ENDIAN_PLATFORM = true;
        private static readonly byte[] DXT1_FROM_LINEAR = [0, 2, 3, 1];
        protected static readonly byte[] DXT5_FROM_LINEAR = [0, 2, 3, 4, 5, 6, 7, 1];
        private static readonly byte[] CRND_CHUNK_ENCODING_NUM_TILES = [1, 2, 2, 3, 3, 3, 3, 4];
        private static readonly byte[][] CRND_CHUNK_ENCODING_TILES = [
            [0, 0, 0, 0],
            [0, 0, 1, 1],
             [0, 1, 0, 1],
            [0, 0, 1, 2],
            [1, 2, 0, 0],
            [0, 1, 0, 2],
             [1, 0, 2, 0],
            [0, 1, 2, 3],
            ];


        protected CrunchHeader _header = new();
        protected SymbolCodec _codec = new();
        protected StaticHuffmanDataModel _chunkEncodingDm = new();
        protected StaticHuffmanDataModel[] _endpointDeltaDm = new StaticHuffmanDataModel[2];
        protected StaticHuffmanDataModel[] _selectorDeltaDm = new StaticHuffmanDataModel[2];
        protected uint[] _colorEndpoints = [];
        protected uint[] _colorSelectors = [];
        protected ushort[] _alphaEndpoints = [];
        protected ushort[] _alphaSelectors = [];


        public byte[] Read(int level_index = 0)
        {
            input.Position = 0;
            _header = ReadHeader();
            if (!InitTables() || !DecodePalettes())
            {
                return [];
            }
            var width = Math.Max(1, _header.Width >> level_index);
            var height = Math.Max(1, _header.Height >> level_index);
            var blocks_x = Math.Max(1, (width + 3) >> 2);
            var blocks_y = Math.Max(1, (height + 3) >> 2);
            var row_pitch = blocks_x * GetBytesPerDxtBlock(_header.Format);
            var total_face_size = row_pitch * blocks_y;
            if (total_face_size < 8 || level_index >= CRNMAX_LEVELS)
            {
                return [];
            }
            var outputLength = total_face_size * _header.Faces;
            var buffer = ArrayPool<byte>.Shared.Rent(outputLength);
            try
            {
                var output = buffer.AsSpan(0, outputLength);
                if (!UnpackLevel(output, row_pitch, level_index))
                {
                    return [];
                }
                return Decode(output, width, height);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
        /// <summary>
        /// 截取部分数据
        /// </summary>
        /// <param name="palette"></param>
        /// <returns></returns>
        protected Stream Slice(CrunchPalette palette)
        {
            return new PartialStream(input, palette.Offset, palette.Size);
        }

        protected virtual byte[] Decode(ReadOnlySpan<byte> output, int width, int height)
        {
            return _header.Format switch
            {
                CrunchFormat.Dxt1 => new BC1().Decode(output, width, height),
                CrunchFormat.CCrnfmtDxt5
                    or CrunchFormat.Dxt5CcxY
                    or CrunchFormat.Dxt5XGbr
                    or CrunchFormat.Dxt5Agbr
                    or CrunchFormat.Dxt5XGxR => new BC3().Decode(output, width, height),
                CrunchFormat.DxnXy or CrunchFormat.DxnYx => new BC5().Decode(output, width, height),
                CrunchFormat.Dxt5a => new BC4().Decode(output, width, height),
                _ => throw new NotImplementedException(),
            };
        }


        #region unpacker

        private bool UnpackLevel(Span<byte> output, int row_pitch_in_bytes, int level_index)
        {
            
            var cur_level_ofs = _header.LevelOffsets[level_index];
            var next_level_ofs = input.Length;
            if (level_index + 1 < _header.LevelOffsets.Length) 
            {
                next_level_ofs = _header.LevelOffsets[level_index + 1];
            }
            if (next_level_ofs <= cur_level_ofs) 
            {
                return false;
            }
            return UnpackLevel(new PartialStream(input, cur_level_ofs, next_level_ofs - cur_level_ofs),
                output,
                row_pitch_in_bytes,
                level_index);
        }
        private bool InitTables()
        {
            bool res;
            res = _codec.StartDecoding(
                new PartialStream(input, _header.TablesOffset, _header.TablesSize)
            );
            if (!res)
            {
                return res;
            }
            res = _codec.DecodeReceiveStaticDataModel(_chunkEncodingDm);
            if (!res)
            {
                return res;
            }
            if (_header.ColorEndpoints.Count == 0 && _header.AlphaEndpoints.Count == 0)
            {
                return false;
            }
            if (_header.ColorEndpoints.Count != 0)
            {
                if (!_codec.DecodeReceiveStaticDataModel(_endpointDeltaDm[0]))
                {
                    return false;
                }
                if (!_codec.DecodeReceiveStaticDataModel(_selectorDeltaDm[0]))
                {
                    return false;
                }
            }
            if (_header.AlphaEndpoints.Count != 0)
            {
                if (!_codec.DecodeReceiveStaticDataModel(_endpointDeltaDm[1]))
                {
                    return false;
                }
                if (!_codec.DecodeReceiveStaticDataModel(_selectorDeltaDm[1]))
                {
                    return false;
                }
            }
            _codec.StopDecoding();
            return true;
        }

        private bool DecodePalettes()
        {
            if (_header.ColorEndpoints.Count != 0)
            {
                if (!DecodeColorEndpoints())
                {
                    return false;
                }
                if (!DecodeColorSelectors())
                {
                    return false;
                }
            }

            if (_header.AlphaEndpoints.Count != 0)
            {
                if (!DecodeAlphaEndpoints())
                {
                    return false;
                }
                if (!DecodeAlphaSelectors())
                {
                    return false;
                }
            }

            return true;
        }

        protected virtual bool DecodeColorEndpoints()
        {
            uint num_color_endpoints = _header.ColorEndpoints.Count;
            _colorEndpoints = new uint[num_color_endpoints];
            bool res;

            res = _codec.StartDecoding(
                Slice(_header.ColorEndpoints)
            );
            if (!res)
            {
                return res;
            }

            var dm = new StaticHuffmanDataModel[2];
            for (int i = 0; i < 2; i++)
            {
                dm[i] = new StaticHuffmanDataModel();
                res = _codec.DecodeReceiveStaticDataModel(dm[i]);
                if (!res)
                {
                    return false;
                }
            }

            uint a = 0, b = 0, c = 0, d = 0, e = 0, f = 0;
            for (int i = 0; i < num_color_endpoints; i++)
            {
                uint da, db, dc, dd, de, df;
                da = _codec.Decode(dm[0]);
                a = (a + da) & 31;
                db = _codec.Decode(dm[1]);
                b = (b + db) & 63;
                dc = _codec.Decode(dm[0]);
                c = (c + dc) & 31;
                dd = _codec.Decode(dm[0]);
                d = (d + dd) & 31;
                de = _codec.Decode(dm[1]);
                e = (e + de) & 63;
                df = _codec.Decode(dm[0]);
                f = (f + df) & 31;
                _colorEndpoints[i] = CRND_LITTLE_ENDIAN_PLATFORM ?
                    (c | (b << 5) | (a << 11) | (f << 16) | (e << 21) | (d << 27)) :
                    (f | (e << 5) | (d << 11) | (c << 16) | (b << 21) | (a << 27));
            }
            _codec.StopDecoding();
            return true;
        }

        protected virtual bool DecodeColorSelectors()
        {
            const uint MAX_SELECTOR_VALUE = 3;
            const int MAX_UNIQUE_SELECTOR_DELTAS = (int)(MAX_SELECTOR_VALUE * 2) + 1;
            int numColorSelectors = (int)_header.ColorSelectors.Count; // Assuming cast_to_uint is a simple cast
            bool res;
            res = _codec.StartDecoding(
                Slice(_header.ColorSelectors)
            );
            if (!res)
            {
                return res;
            }
            var dm = new StaticHuffmanDataModel();
            res = _codec.DecodeReceiveStaticDataModel(dm);
            if (!res)
            {
                return res;
            }
            int[] delta0 = new int[MAX_UNIQUE_SELECTOR_DELTAS * MAX_UNIQUE_SELECTOR_DELTAS];
            int[] delta1 = new int[MAX_UNIQUE_SELECTOR_DELTAS * MAX_UNIQUE_SELECTOR_DELTAS];
            int l = -(int)MAX_SELECTOR_VALUE;
            int m = -(int)MAX_SELECTOR_VALUE;
            for (int i = 0; i < MAX_UNIQUE_SELECTOR_DELTAS * MAX_UNIQUE_SELECTOR_DELTAS; i++)
            {
                delta0[i] = l;
                delta1[i] = m;
                l += 1;
                if (l > (int)MAX_SELECTOR_VALUE)
                {
                    l = -(int)MAX_SELECTOR_VALUE;
                    m += 1;
                }
            }
            var cur = new int[16];
            _colorSelectors = new uint[numColorSelectors];
            var pFromLinear = DXT1_FROM_LINEAR;
            for (int i = 0; i < numColorSelectors; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    uint sym = _codec.Decode(dm);
                    cur[j * 2] = (delta0[sym] + cur[j * 2]) & 3;
                    cur[j * 2 + 1] = (delta1[sym] + cur[j * 2 + 1]) & 3;
                }
                if (CRND_LITTLE_ENDIAN_PLATFORM)
                {
                    _colorSelectors[i] = ((uint)pFromLinear[cur[0]])
                        | ((uint)pFromLinear[cur[1]] << 2)
                        | ((uint)pFromLinear[cur[2]] << 4)
                        | (((uint)pFromLinear[cur[3]]) << 6)
                        | (((uint)pFromLinear[cur[4]]) << 8)
                        | (((uint)pFromLinear[cur[5]]) << 10)
                        | (((uint)pFromLinear[cur[6]]) << 12)
                        | (((uint)pFromLinear[cur[7]]) << 14)
                        | (((uint)pFromLinear[cur[8]]) << 16)
                        | (((uint)pFromLinear[cur[9]]) << 18)
                        | (((uint)pFromLinear[cur[10]]) << 20)
                        | (((uint)pFromLinear[cur[11]]) << 22)
                        | (((uint)pFromLinear[cur[12]]) << 24)
                        | (((uint)pFromLinear[cur[13]]) << 26)
                        | (((uint)pFromLinear[cur[14]]) << 28)
                        | (((uint)pFromLinear[cur[15]]) << 30);
                }
                else
                {
                    _colorSelectors[i] = ((uint)pFromLinear[cur[8]])
                        | (((uint)pFromLinear[cur[9]]) << 2)
                        | (((uint)pFromLinear[cur[10]]) << 4)
                        | (((uint)pFromLinear[cur[11]]) << 6)
                        | (((uint)pFromLinear[cur[12]]) << 8)
                        | (((uint)pFromLinear[cur[13]]) << 10)
                        | (((uint)pFromLinear[cur[14]]) << 12)
                        | (((uint)pFromLinear[cur[15]]) << 14)
                        | (((uint)pFromLinear[cur[0]]) << 16)
                        | (((uint)pFromLinear[cur[1]]) << 18)
                        | (((uint)pFromLinear[cur[2]]) << 20)
                        | (((uint)pFromLinear[cur[3]]) << 22)
                        | (((uint)pFromLinear[cur[4]]) << 24)
                        | (((uint)pFromLinear[cur[5]]) << 26)
                        | (((uint)pFromLinear[cur[6]]) << 28)
                        | (((uint)pFromLinear[cur[7]]) << 30);
                }
            }
            _codec.StopDecoding();
            return true;
        }

        private bool DecodeAlphaEndpoints()
        {
            int numAlphaEndpoints = (int)_header.AlphaEndpoints.Count; // Assuming cast_to_uint is a simple cast
            bool res;
            res = _codec.StartDecoding(
                Slice(_header.AlphaEndpoints)
            );
            if (!res)
            {
                return res;
            }
            var dm = new StaticHuffmanDataModel();
            res = _codec.DecodeReceiveStaticDataModel(dm);
            if (!res)
            {
                return res;
            }
            _alphaEndpoints = new ushort[numAlphaEndpoints];
            uint a = 0;
            uint b = 0;
            for (int i = 0; i < numAlphaEndpoints; i++)
            {
                uint sa = _codec.Decode(dm);
                uint sb = _codec.Decode(dm);
                a = (sa + a) & 0xFF;
                b = (sb + b) & 0xFF;
                _alphaEndpoints[i] = (ushort)(a | (b << 8));
            }
            _codec.StopDecoding();
            return true;
        }

        protected virtual bool DecodeAlphaSelectors()
        {
            const uint MAX_SELECTOR_VALUE = 7;
            const int MAX_UNIQUE_SELECTOR_DELTAS = (int)(MAX_SELECTOR_VALUE * 2 + 1);
            int numAlphaSelectors = (int)_header.AlphaSelectors.Count;
            bool res;
            res = _codec.StartDecoding(
                Slice(_header.AlphaSelectors)
            );
            if (!res)
            {
                return res;
            }
            var dm = new StaticHuffmanDataModel();
            res = _codec.DecodeReceiveStaticDataModel(dm);
            if (!res)
            {
                return res;
            }
            int[] delta0 = new int[MAX_UNIQUE_SELECTOR_DELTAS * MAX_UNIQUE_SELECTOR_DELTAS];
            int[] delta1 = new int[MAX_UNIQUE_SELECTOR_DELTAS * MAX_UNIQUE_SELECTOR_DELTAS];
            int l = -(int)MAX_SELECTOR_VALUE;
            int m = -(int)MAX_SELECTOR_VALUE;
            for (int i = 0; i < MAX_UNIQUE_SELECTOR_DELTAS * MAX_UNIQUE_SELECTOR_DELTAS; i++)
            {
                delta0[i] = l;
                delta1[i] = m;
                l += 1;
                if (l > (int)MAX_SELECTOR_VALUE)
                {
                    l = -(int)MAX_SELECTOR_VALUE;
                    m += 1;
                }
            }
            var cur = new uint[16];
            _alphaSelectors = new ushort[numAlphaSelectors * 3];
            var pFromLinear = DXT5_FROM_LINEAR;
            for (int i = 0; i < numAlphaSelectors; i++)
            {
                for (uint j = 0; j < 8; j++)
                {
                    int sym = (int)_codec.Decode(dm);
                    cur[j * 2] = (uint)((delta0[sym] + (int)cur[j * 2]) & 7);
                    cur[j * 2 + 1] = (uint)((delta1[sym] + (int)cur[j * 2 + 1]) & 7);
                }
                _alphaSelectors[i * 3] = (ushort)(pFromLinear[cur[0]]
                    | (pFromLinear[cur[1]] << 3)
                    | (pFromLinear[cur[2]] << 6)
                    | (pFromLinear[cur[3]] << 9)
                    | (pFromLinear[cur[4]] << 12)
                    | (pFromLinear[cur[5]] << 15));

                _alphaSelectors[i * 3 + 1] = (ushort)((pFromLinear[cur[5]] >> 1)
                    | (pFromLinear[cur[6]] << 2)
                    | (pFromLinear[cur[7]] << 5)
                    | (pFromLinear[cur[8]] << 8)
                    | (pFromLinear[cur[9]] << 11)
                    | (pFromLinear[cur[10]] << 14));

                _alphaSelectors[i * 3 + 2] = (ushort)((pFromLinear[cur[10]] >> 2)
                    | (pFromLinear[cur[11]] << 1)
                    | (pFromLinear[cur[12]] << 4)
                    | (pFromLinear[cur[13]] << 7)
                    | (pFromLinear[cur[14]] << 10)
                    | (pFromLinear[cur[15]] << 13));
            }
            _codec.StopDecoding();
            return true;
        }

        private bool UnpackLevel(
            Stream source,
            Span<byte> output,
            int rowPitchInBytes,
            int levelIndex)
        {
            int width = Math.Max(_header.Width >> levelIndex, 1);
            int height = Math.Max(_header.Height >> levelIndex, 1);
            int blocksX = (width + 3) >> 2;
            int blocksY = (height + 3) >> 2;
            int blockSize = GetBlockSize(_header.Format);
            var minimalRowPitch = blockSize * blocksX;

            if (rowPitchInBytes == 0)
            {
                rowPitchInBytes = minimalRowPitch;
            }
            else if (rowPitchInBytes < minimalRowPitch || (rowPitchInBytes & 3) != 0)
            {
                return false;
            }

            if (output.Length < rowPitchInBytes * blocksY)
            {
                return false;
            }

            if (!_codec.StartDecoding(source))
            {
                return false;
            }

            Unpack(output, rowPitchInBytes, blocksX, blocksY);

            _codec.StopDecoding();
            return true;
        }

        protected virtual void Unpack(Span<byte> output, int rowPitchInBytes, int blocksWidth, int blocksHeight)
        {
            var chunksX = (blocksWidth + 1) >> 1;
            var chunksY = (blocksHeight + 1) >> 1;
            switch (_header.Format)
            {
                case CrunchFormat.Dxt1:
                    UnpackDxt1(output, rowPitchInBytes, blocksWidth, blocksHeight, chunksX, chunksY);
                    break;

                case CrunchFormat.CCrnfmtDxt5:
                case CrunchFormat.Dxt5CcxY:
                case CrunchFormat.Dxt5XGbr:
                case CrunchFormat.Dxt5Agbr:
                case CrunchFormat.Dxt5XGxR:
                    UnpackDxt5(output, rowPitchInBytes, blocksWidth, blocksHeight, chunksX, chunksY);
                    break;

                case CrunchFormat.Dxt5a:
                    UnpackDxt5a(output, rowPitchInBytes, blocksWidth, blocksHeight, chunksX, chunksY);
                    break;

                case CrunchFormat.DxnXy:
                case CrunchFormat.DxnYx:
                    UnpackDxn(output, rowPitchInBytes, blocksWidth, blocksHeight, chunksX, chunksY);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
        #endregion

        #region Dxt
        private bool UnpackDxt1(
            Span<byte> output,
            int rowPitchInBytes,
            int blocksX,
            int blocksY,
            int chunksX,
            int chunksY)
        {
            uint chunkEncodingBits = 1;
            uint numColorEndpoints = (uint)_colorEndpoints.Length;
            uint numColorSelectors = (uint)_colorSelectors.Length;
            uint prevColorEndpointIndex = 0;
            uint prevColorSelectorIndex = 0;
            uint numFaces = _header.Faces;
            int rowPitchInDwords = rowPitchInBytes >> 2;
            int cBytesPerBlock = 8;
            int rowDst = 0;

            for (int f = 0; f < numFaces; f++)
            {
                for (uint y = 0; y < chunksY; y++)
                {
                    int blockDst = rowDst;
                    IEnumerable<int> iter;
                    int blockDelta;

                    if (y % 2 == 1)
                    {
                        iter = Enumerable.Range(0, chunksX).Reverse();
                        blockDelta = -cBytesPerBlock * 2;
                        blockDst += (chunksX - 1) * cBytesPerBlock * 2;
                    }
                    else
                    {
                        blockDelta = cBytesPerBlock * 2;
                        iter = Enumerable.Range(0, chunksX);
                    }

                    bool skipBottomRow = (y == (chunksY - 1)) && ((blocksY % 2) == 1);
                    foreach (int x in iter)
                    {
                        var colorEndpoints = new uint[4];
                        if (chunkEncodingBits == 1)
                        {
                            chunkEncodingBits = _codec.Decode(_chunkEncodingDm);
                            chunkEncodingBits |= 512;
                        }

                        uint chunkEncodingIndex = chunkEncodingBits & 7;
                        chunkEncodingBits >>= 3;

                        uint numTiles = CRND_CHUNK_ENCODING_NUM_TILES[chunkEncodingIndex];
                        for (int i = 0; i < numTiles; i++)
                        {
                            uint delta = _codec.Decode(_endpointDeltaDm[0]);
                            prevColorEndpointIndex = Math.Min(prevColorEndpointIndex + delta, numColorEndpoints);
                            colorEndpoints[i] = _colorEndpoints[prevColorEndpointIndex];
                        }

                        var pTileIndices = CRND_CHUNK_ENCODING_TILES[chunkEncodingIndex];
                        bool skipRightCol = ((blocksX % 2) == 1) && (x == (chunksX - 1));
                        var pdDst = blockDst >> 2;

                        if (!skipBottomRow && !skipRightCol)
                        {
                            WriteUInt(output, pdDst, colorEndpoints[pTileIndices[0]]);
                            uint delta0 = _codec.Decode(_selectorDeltaDm[0]);
                            prevColorSelectorIndex = Math.Min(prevColorSelectorIndex + delta0, numColorSelectors);
                            WriteUInt(output, pdDst + 1, _colorSelectors[prevColorSelectorIndex]);
                            WriteUInt(output, pdDst + 2, colorEndpoints[pTileIndices[1]]);

                            uint delta1 = _codec.Decode(_selectorDeltaDm[0]);
                            prevColorSelectorIndex = Math.Min(prevColorSelectorIndex + delta1, numColorSelectors);
                            WriteUInt(output, pdDst + 3, _colorSelectors[prevColorSelectorIndex]);
                            WriteUInt(output, pdDst + rowPitchInDwords, colorEndpoints[pTileIndices[2]]);

                            uint delta2 = _codec.Decode(_selectorDeltaDm[0]);
                            prevColorSelectorIndex = Math.Min(prevColorSelectorIndex + delta2, numColorSelectors);
                            WriteUInt(output, pdDst + 1 + rowPitchInDwords, _colorSelectors[prevColorSelectorIndex]);
                            WriteUInt(output, pdDst + 2 + rowPitchInDwords, colorEndpoints[pTileIndices[3]]);

                            uint delta3 = _codec.Decode(_selectorDeltaDm[0]);
                            prevColorSelectorIndex = Math.Min(prevColorSelectorIndex + delta3, numColorSelectors);
                            WriteUInt(output, pdDst + 3 + rowPitchInDwords, _colorSelectors[prevColorSelectorIndex]);
                        }
                        else
                        {
                            for (int by = 0; by < 2; by++)
                            {
                                pdDst = blockDst + (rowPitchInBytes * by);
                                pdDst >>= 2;
                                for (int bx = 0; bx < 2; bx++)
                                {
                                    uint delta = _codec.Decode(_selectorDeltaDm[0]);
                                    prevColorSelectorIndex = Math.Min(prevColorSelectorIndex + delta, numColorSelectors);
                                    if (!(((bx != 0) && skipRightCol) || ((by != 0) && skipBottomRow)))
                                    {
                                        WriteUInt(output, pdDst, colorEndpoints[pTileIndices[bx + by * 2]]);
                                        WriteUInt(output, pdDst + 1, _colorSelectors[prevColorSelectorIndex]);
                                    }
                                    pdDst += 2;
                                }
                            }
                        }
                        blockDst += blockDelta;
                    }
                    rowDst += rowPitchInBytes * 2;
                }
            }
            return true;
        }

        private bool UnpackDxt5(
            Span<byte> output,
            int rowPitchInBytes,
            int blocksX,
            int blocksY,
            int chunksX,
            int chunksY)
        {
            uint chunkEncodingBits = 1;
            uint numColorEndpoints = (uint)_colorEndpoints.Length;
            uint numColorSelectors = (uint)_colorSelectors.Length;
            uint numAlphaEndpoints = (uint)_alphaEndpoints.Length;
            uint numAlphaSelectors = _header.AlphaSelectors.Count;
            uint prevColorEndpointIndex = 0;
            uint prevColorSelectorIndex = 0;
            uint prevAlphaEndpointIndex = 0;
            uint prevAlphaSelectorIndex = 0;
            uint numFaces = _header.Faces;
            int cBytesPerBlock = 16;
            int rowDst = 0;

            for (int f = 0; f < numFaces; f++)
            {
                for (uint y = 0; y < chunksY; y++)
                {
                    int blockDst = rowDst;
                    IEnumerable<int> iter;
                    int blockDelta;

                    if (y % 2 == 1)
                    {
                        iter = Enumerable.Range(0, chunksX).Reverse();
                        blockDelta = -cBytesPerBlock * 2;
                        blockDst += (chunksX - 1) * cBytesPerBlock * 2;
                    }
                    else
                    {
                        blockDelta = cBytesPerBlock * 2;
                        iter = Enumerable.Range(0, chunksX);
                    }

                    bool skipBottomRow = (y == (chunksY - 1)) && ((blocksY % 2) == 1);
                    foreach (int x in iter)
                    {
                        uint[] colorEndpoints = new uint[4];
                        uint[] alphaEndpoints = new uint[4];

                        if (chunkEncodingBits == 1)
                        {
                            chunkEncodingBits = _codec.Decode(_chunkEncodingDm);
                            chunkEncodingBits |= 512;
                        }

                        uint chunkEncodingIndex = chunkEncodingBits & 7;
                        chunkEncodingBits >>= 3;
                        uint numTiles = CRND_CHUNK_ENCODING_NUM_TILES[chunkEncodingIndex];
                        var pTileIndices = CRND_CHUNK_ENCODING_TILES[chunkEncodingIndex];
                        bool skipRightCol = (blocksX % 2) != 0 && (x == (chunksX - 1));

                        for (uint i = 0; i < numTiles; i++)
                        {
                            uint delta = _codec.Decode(_endpointDeltaDm[1]);
                            prevAlphaEndpointIndex = Math.Min(prevAlphaEndpointIndex + delta, numAlphaEndpoints);
                            alphaEndpoints[i] = _alphaEndpoints[prevAlphaEndpointIndex];
                        }

                        for (uint i = 0; i < numTiles; i++)
                        {
                            uint delta = _codec.Decode(_endpointDeltaDm[0]);
                            prevColorEndpointIndex = Math.Min(prevColorEndpointIndex + delta, numColorEndpoints);
                            colorEndpoints[i] = _colorEndpoints[prevColorEndpointIndex];
                        }

                        int pdDst = blockDst >> 2;
                        for (int by = 0; by < 2; by++)
                        {
                            for (int bx = 0; bx < 2; bx++)
                            {
                                uint delta0 = _codec.Decode(_selectorDeltaDm[1]);
                                prevAlphaSelectorIndex = Math.Min(prevAlphaSelectorIndex + delta0, numAlphaSelectors);

                                uint delta1 = _codec.Decode(_selectorDeltaDm[0]);
                                prevColorSelectorIndex = Math.Min(prevColorSelectorIndex + delta1, numColorSelectors);

                                if (!((bx != 0 && skipRightCol) || (by != 0 && skipBottomRow)))
                                {
                                    uint tileIndex = pTileIndices[bx + by * 2];
                                    var pAlphaSelectors = _alphaSelectors.Skip((int)(prevAlphaSelectorIndex * 3)).ToArray();

                                    // Assuming WriteUInt is a method to write to the buffer
                                    WriteUInt(output, pdDst + 0, (alphaEndpoints[tileIndex] << 16) | pAlphaSelectors[0]);
                                    WriteUInt(output, pdDst + 1, (pAlphaSelectors[1] | ((uint)pAlphaSelectors[2] << 16)));
                                    WriteUInt(output, pdDst + 2, colorEndpoints[tileIndex]);
                                    WriteUInt(output, pdDst + 3, _colorSelectors[prevColorSelectorIndex]);
                                }
                                pdDst += 4;
                            }
                            pdDst <<= 2;
                            pdDst = (pdDst + (-cBytesPerBlock * 2) + rowPitchInBytes) >> 2;
                        }
                        blockDst += blockDelta;
                    }
                    rowDst += rowPitchInBytes * 2;
                }
            }
            return true;
        }

        private bool UnpackDxt5a(
            Span<byte> output,
            int rowPitchInBytes,
            int blocksX,
            int blocksY,
            int chunksX,
            int chunksY)
        {
            uint chunkEncodingBits = 1;
            uint numAlphaEndpoints = (uint)_alphaEndpoints.Length;
            uint numAlphaSelectors = _header.AlphaSelectors.Count; // Assuming cast_to_uint is not needed in C#
            uint prevAlpha0EndpointIndex = 0;
            uint prevAlpha0SelectorIndex = 0;
            uint numFaces = _header.Faces; // Assuming cast_to_uint is not needed in C#
            const int cBytesPerBlock = 8;
            int rowDst = 0;

            for (int f = 0; f < numFaces; f++)
            {
                for (uint y = 0; y < chunksY; y++)
                {
                    int blockDst = rowDst;
                    IEnumerable<int> iter;
                    int blockDelta;

                    if (y % 2 == 1)
                    {
                        iter = Enumerable.Range(0, chunksX).Reverse();
                        blockDelta = -cBytesPerBlock * 2;
                        blockDst += (chunksX - 1) * cBytesPerBlock * 2;
                    }
                    else
                    {
                        blockDelta = cBytesPerBlock * 2;
                        iter = Enumerable.Range(0, chunksX);
                    }

                    bool skipBottomRow = (y == (chunksY - 1)) && ((blocksY % 2) == 1);
                    foreach (int x in iter)
                    {
                        uint[] alpha0Endpoints = new uint[4];

                        if (chunkEncodingBits == 1)
                        {
                            chunkEncodingBits = _codec.Decode(_chunkEncodingDm);
                            chunkEncodingBits |= 512;
                        }

                        uint chunkEncodingIndex = chunkEncodingBits & 7;
                        chunkEncodingBits >>= 3;
                        uint numTiles = CRND_CHUNK_ENCODING_NUM_TILES[chunkEncodingIndex];
                        var pTileIndices = CRND_CHUNK_ENCODING_TILES[chunkEncodingIndex];
                        bool skipRightCol = (blocksX % 2) != 0 && (x == (chunksX - 1));

                        for (uint i = 0; i < numTiles; i++)
                        {
                            uint delta = _codec.Decode(_endpointDeltaDm[1]);
                            prevAlpha0EndpointIndex = Math.Min(prevAlpha0EndpointIndex + delta, numAlphaEndpoints);
                            alpha0Endpoints[i] = _alphaEndpoints[prevAlpha0EndpointIndex];
                        }

                        int pdDst = blockDst >> 2;
                        for (int by = 0; by < 2; by++)
                        {
                            for (int bx = 0; bx < 2; bx++)
                            {
                                uint delta = _codec.Decode(_selectorDeltaDm[1]);
                                prevAlpha0SelectorIndex = Math.Min(prevAlpha0SelectorIndex + delta, numAlphaSelectors);

                                if (!(((bx != 0) && skipRightCol) || ((by != 0) && skipBottomRow)))
                                {
                                    uint tileIndex = pTileIndices[bx + by * 2];
                                    var pAlpha0Selectors = _alphaSelectors.Skip((int)(prevAlpha0SelectorIndex * 3)).ToArray();

                                    // Assuming WriteUInt is a method to write to the buffer
                                    WriteUInt(output, pdDst + 0, (alpha0Endpoints[tileIndex] << 16) | pAlpha0Selectors[0]);
                                    WriteUInt(output, pdDst + 1, ((uint)pAlpha0Selectors[1] << 16) | pAlpha0Selectors[2]);
                                }
                                pdDst += 2;
                            }
                            pdDst <<= 2;
                            pdDst = (pdDst + (-cBytesPerBlock * 2) + rowPitchInBytes) >> 2;
                        }
                        blockDst += blockDelta;
                    }
                    rowDst += rowPitchInBytes * 2;
                }
            }
            return true;
        }

        private bool UnpackDxn(
            Span<byte> output,
            int rowPitchInBytes,
            int blocksX,
            int blocksY,
            int chunksX,
            int chunksY)
        {
            uint chunkEncodingBits = 1;
            uint numAlphaEndpoints = (uint)_alphaEndpoints.Length;
            uint numAlphaSelectors = _header.AlphaSelectors.Count;
            uint prevAlpha0EndpointIndex = 0;
            uint prevAlpha0SelectorIndex = 0;
            uint prevAlpha1EndpointIndex = 0;
            uint prevAlpha1SelectorIndex = 0;
            uint numFaces = _header.Faces;
            int cBytesPerBlock = 16;
            int rowDst = 0;

            for (int f = 0; f < numFaces; f++)
            {
                for (uint y = 0; y < chunksY; y++)
                {
                    int blockDst = rowDst;
                    IEnumerable<int> iter;
                    int blockDelta;

                    if (y % 2 == 1)
                    {
                        iter = Enumerable.Range(0, chunksX).Reverse();
                        blockDelta = -cBytesPerBlock * 2;
                        blockDst += (chunksX - 1) * cBytesPerBlock * 2;
                    }
                    else
                    {
                        blockDelta = cBytesPerBlock * 2;
                        iter = Enumerable.Range(0, chunksX);
                    }

                    bool skipBottomRow = (y == (chunksY - 1)) && ((blocksY % 2) == 1);
                    foreach (int x in iter)
                    {
                        uint[] alpha0Endpoints = new uint[4];
                        uint[] alpha1Endpoints = new uint[4];

                        if (chunkEncodingBits == 1)
                        {
                            chunkEncodingBits = _codec.Decode(_chunkEncodingDm);
                            chunkEncodingBits |= 512;
                        }

                        uint chunkEncodingIndex = chunkEncodingBits & 7;
                        chunkEncodingBits >>= 3;
                        uint numTiles = CRND_CHUNK_ENCODING_NUM_TILES[chunkEncodingIndex];
                        var pTileIndices = CRND_CHUNK_ENCODING_TILES[chunkEncodingIndex];
                        bool skipRightCol = (blocksX % 2) != 0 && (x == (chunksX - 1));

                        for (uint i = 0; i < numTiles; i++)
                        {
                            uint delta = _codec.Decode(_endpointDeltaDm[1]);
                            prevAlpha0EndpointIndex = Math.Min(prevAlpha0EndpointIndex + delta, numAlphaEndpoints);
                            alpha0Endpoints[i] = _alphaEndpoints[prevAlpha0EndpointIndex];
                        }

                        for (uint i = 0; i < numTiles; i++)
                        {
                            uint delta = _codec.Decode(_endpointDeltaDm[1]);
                            prevAlpha1EndpointIndex = Math.Min(prevAlpha1EndpointIndex + delta, numAlphaEndpoints);
                            alpha1Endpoints[i] = _alphaEndpoints[prevAlpha1EndpointIndex];
                        }

                        int pdDst = blockDst >> 2;
                        for (int by = 0; by < 2; by++)
                        {
                            for (int bx = 0; bx < 2; bx++)
                            {
                                uint delta0 = _codec.Decode(_selectorDeltaDm[1]);
                                prevAlpha0SelectorIndex = Math.Min(prevAlpha0SelectorIndex + delta0, numAlphaSelectors);

                                uint delta1 = _codec.Decode(_selectorDeltaDm[1]);
                                prevAlpha1SelectorIndex = Math.Min(prevAlpha1SelectorIndex + delta1, numAlphaSelectors);

                                if (!(((bx != 0) && skipRightCol) || ((by != 0) && skipBottomRow)))
                                {
                                    uint tileIndex = pTileIndices[bx + by * 2];
                                    var pAlpha0Selectors = _alphaSelectors.Skip((int)(prevAlpha0SelectorIndex * 3)).ToArray();
                                    var pAlpha1Selectors = _alphaSelectors.Skip((int)(prevAlpha1SelectorIndex * 3)).ToArray();

                                    // Assuming WriteUInt is a method to write to the buffer
                                    WriteUInt(output, pdDst + 0, (alpha0Endpoints[tileIndex] << 16) | pAlpha0Selectors[0]);
                                    WriteUInt(output, pdDst + 1, (pAlpha0Selectors[1] | ((uint)pAlpha0Selectors[2] << 16)));
                                    WriteUInt(output, pdDst + 2, (alpha1Endpoints[tileIndex] << 16) | pAlpha1Selectors[0]);
                                    WriteUInt(output, pdDst + 3, (pAlpha1Selectors[1] | ((uint)pAlpha1Selectors[2] << 16)));
                                }
                                pdDst += 4;
                            }
                            pdDst <<= 2;
                            pdDst = (pdDst + (-cBytesPerBlock * 2) + rowPitchInBytes) >> 2;
                        }
                        blockDst += blockDelta;
                    }
                    rowDst += rowPitchInBytes * 2;
                }
            }
            return true;
        }

        protected static void WriteUInt(Span<byte> output, int index, uint val)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(output[(index * 4)..], val);
        }
        #endregion

        private CrunchHeader ReadHeader()
        {
            var reader = new EndianReader(input, EndianType.BigEndian);
            var res = new CrunchHeader();
            Expectation.ThrowIfNotSignature(reader.ReadBytes(2).SequenceEqual(CrunchHeader.Signature));
            res.HeaderSize = reader.ReadUInt16();
            res.HeaderCrc = reader.ReadUInt16();
            res.DataSize = reader.ReadUInt32();
            res.DataCrc = reader.ReadUInt16();
            res.Width = reader.ReadUInt16();
            res.Height = reader.ReadUInt16();
            var levelCount = reader.ReadByte();
            res.Faces = reader.ReadByte();
            res.Format = (CrunchFormat)reader.ReadByte();
            res.Flags  = reader.ReadUInt16();
            res.Reserved = reader.ReadUInt32();
            res.Userdata0 = reader.ReadUInt32();
            res.Userdata1 = reader.ReadUInt32();
            res.ColorEndpoints = ReadPalette(reader);
            res.ColorSelectors = ReadPalette(reader);
            res.AlphaEndpoints = ReadPalette(reader);
            res.AlphaSelectors = ReadPalette(reader);
            res.TablesSize = reader.ReadUInt16();
            res.TablesOffset = ReadUInt3(reader);
            res.LevelOffsets = reader.ReadArray(levelCount, reader.ReadUInt32);
            Expectation.ThrowIf(res.HeaderSize < CrunchHeader.MIN_SIZE);
            return res;
        }

        private CrunchPalette ReadPalette(BinaryReader reader)
        {
            return new CrunchPalette(ReadUInt3(reader), ReadUInt3(reader), reader.ReadUInt16());
        }

        private static uint ReadUInt3(BinaryReader reader)
        {
            return (uint)reader.ReadByte() << 16 | reader.ReadUInt16();
        }

        private int GetBytesPerDxtBlock(CrunchFormat format)
        {
            return (GetBitsPerTexel(format) << 4) >> 3;
        }

        protected virtual int GetBlockSize(CrunchFormat format)
        {
            return format is CrunchFormat.Dxt1 or CrunchFormat.Dxt5a ? 8 : 16;
        }

        protected virtual int GetBitsPerTexel(CrunchFormat format)
        {
            return format switch
            {
                CrunchFormat.Dxt1 or CrunchFormat.Dxt5a or CrunchFormat.Etc1 => 4,

                CrunchFormat.Dxt3
                or CrunchFormat.CCrnfmtDxt5
                or CrunchFormat.DxnXy
                or CrunchFormat.DxnYx
                or CrunchFormat.Dxt5CcxY
                or CrunchFormat.Dxt5XGxR
                or CrunchFormat.Dxt5XGbr
                or CrunchFormat.Dxt5Agbr => 8,
                _ => throw new NotSupportedException(format.ToString()),
            };
        }

        public void Dispose()
        {
            input.Dispose();
        }
    }
}
