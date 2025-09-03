using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO;
using System.Linq;
using ZoDream.Shared.Drawing;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.Compression
{
    public class UnityCrunch(Stream input): Crunch(input)
    {

        protected override byte[] Decode(ReadOnlySpan<byte> output, int width, int height)
        {
            return _header.Format switch
            {
                CrunchFormat.Dxt1 => new BC1().Decode(output, width, height),
                CrunchFormat.Etc1 or CrunchFormat.Etc1s => new ETC().Decode(output, width, height),
                CrunchFormat.CCrnfmtDxt5
                or CrunchFormat.Dxt5CcxY
                or CrunchFormat.Dxt5XGbr
                or CrunchFormat.Dxt5Agbr
                or CrunchFormat.Dxt5XGxR => new BC3().Decode(output, width, height),
                CrunchFormat.DxnXy or CrunchFormat.DxnYx => new BC5().Decode(output, width, height),
                CrunchFormat.Dxt5a => new BC4().Decode(output, width, height),
                CrunchFormat.Etc2 => new ETC2().Decode(output, width, height),
                CrunchFormat.Etc2a or CrunchFormat.Etc2as => new ETC2A8().Decode(output, width, height),
                _ => throw new NotImplementedException(),
            };
        }


        #region unpacker


        protected override bool DecodeColorEndpoints()
        {
            bool has_etc_color_blocks = _header.Format == CrunchFormat.Etc1 ||
                                        _header.Format == CrunchFormat.Etc2 ||
                                        _header.Format == CrunchFormat.Etc2a ||
                                        _header.Format == CrunchFormat.Etc1s ||
                                        _header.Format == CrunchFormat.Etc2as;
            if (!has_etc_color_blocks)
            {
                return base.DecodeColorEndpoints();
            }
            uint num_color_endpoints = _header.ColorEndpoints.Count;
            

            bool has_subblocks = _header.Format == CrunchFormat.Etc1 ||
                                 _header.Format == CrunchFormat.Etc2 ||
                                 _header.Format == CrunchFormat.Etc2a;
            _colorEndpoints = new uint[num_color_endpoints];
            if (!_codec.StartDecoding(
                    Slice(_header.ColorEndpoints)
                ))
            {
                return false;
            }

            var dm = new StaticHuffmanDataModel();
            if (!_codec.DecodeReceiveStaticDataModel(dm))
            {
                return false;
            }

            uint a = 0, b = 0, c = 0, d = 0, e = 0, f = 0;
            for (uint i = 0; i < num_color_endpoints; i++)
            {
                for (int shift = 0; shift < 4; shift++)
                {
                    a += _codec.Decode(dm) << (shift * 8);
                }
                a &= 0x1F1F1F1F;
                if (has_subblocks)
                {
                    _colorEndpoints[0] = a;
                }
                else
                {
                    _colorEndpoints[0] = (a & 0x07000000) << 5 |
                                (a & 0x07000000) << 2 |
                                0x02000000 |
                                (a & 0x001F1F1F) << 3;
                }
            }
            _codec.StopDecoding();
            return true;
        }

        protected override bool DecodeColorSelectors()
        {
            bool hasEtcColorBlocks = (_header.Format == CrunchFormat.Etc1) ||
                                     (_header.Format == CrunchFormat.Etc2) ||
                                     (_header.Format == CrunchFormat.Etc2a) ||
                                     (_header.Format == CrunchFormat.Etc1s) ||
                                     (_header.Format == CrunchFormat.Etc2as);

            bool hasSubblocks = (_header.Format == CrunchFormat.Etc1) ||
                                (_header.Format == CrunchFormat.Etc2) ||
                                (_header.Format == CrunchFormat.Etc2a);
            bool res;
            // Return value here is ignored in the original code.
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
            int numColorSelectors = (int)_header.ColorSelectors.Count;
            if (hasSubblocks)
            {
                numColorSelectors <<= 1;
            }
            _colorSelectors = new uint[numColorSelectors];
            uint s = 0;
            for (int i = 0; i < (int)_header.ColorSelectors.Count; i++)
            {
                foreach (int j in new int[] { 0, 4, 8, 12, 16, 20, 24, 28 })
                {
                    s ^= _codec.Decode(dm) << j;
                }
                if (hasEtcColorBlocks)
                {
                    uint selector = (~s & 0xAAAAAAAA) | ((~(s ^ (s >> 1)) & 0x55555555));
                    int t = 8;
                    for (int h = 0; h < 4; h++)
                    {
                        for (int w = 0; w < 4; w++)
                        {
                            if (hasSubblocks)
                            {
                                uint s0 = selector >> (w << 3 | h << 1);
                                _colorSelectors[i << 1] |= ((s0 >> 1 & 1) | (s0 & 1) << 16) << (t & 15);
                            }
                            uint s1 = selector >> (h << 3 | w << 1);
                            if (hasSubblocks)
                            {
                                _colorSelectors[i << 1 | 1] |= ((s1 >> 1 & 1) | (s1 & 1) << 16) << (t & 15);
                            }
                            else
                            {
                                _colorSelectors[i] |= ((s1 >> 1 & 1) | (s1 & 1) << 16) << (t & 15);
                            }
                            t += 4;
                        }
                        t -= 15;
                    }
                }
                else
                {
                    _colorSelectors[i] = ((s ^ (s << 1)) & 0xAAAAAAAA) | (s >> 1 & 0x55555555);
                }
            }
            _codec.StopDecoding();
            return true;
        }

        protected override bool DecodeAlphaSelectors()
        {
            if (_header.Format == CrunchFormat.Etc2as)
            {

                return DecodeAlphaSelectorsEtcs();
            }
            else if (_header.Format == CrunchFormat.Etc2a)
            {
                return DecodeAlphaSelectorsEtc();
            }
            bool res = _codec.StartDecoding(
                new PartialStream(input, _header.AlphaSelectors.Offset, _header.AlphaSelectors.Size)
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
            int numAlphaSelectors = (int)_header.AlphaSelectors.Count;
            _alphaSelectors = new ushort[numAlphaSelectors * 3];
            byte[] dxt5FromLinear = new byte[64];
            for (int i = 0; i < 64; i++)
            {
                dxt5FromLinear[i] = (byte)(DXT5_FROM_LINEAR[i & 7] | (DXT5_FROM_LINEAR[i >> 3] << 3));
            }
            uint s0Linear = 0;
            uint s1Linear = 0;
            int index = 0;
            while (index < _alphaSelectors.Length)
            {
                uint s0 = 0;
                uint s1 = 0;
                for (int j = 0; j < 4; j++)
                {
                    s0Linear ^= _codec.Decode(dm) << (j * 6);
                    s0 |= (uint)(dxt5FromLinear[s0Linear >> (j * 6) & 0x3F] << (j * 6));
                }
                for (int j = 0; j < 4; j++)
                {
                    s1Linear ^= _codec.Decode(dm) << (j * 6);
                    s1 |= (uint)(dxt5FromLinear[s1Linear >> (j * 6) & 0x3F] << (j * 6));
                }
                _alphaSelectors[index++] = (ushort)s0;
                _alphaSelectors[index++] = (ushort)((s0 >> 16) | (s1 << 8));
                _alphaSelectors[index++] = (ushort)(s1 >> 8);
            }
            _codec.StopDecoding();
            return true;
        }

        private bool DecodeAlphaSelectorsEtc()
        {
            bool res = _codec.StartDecoding(
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
            // + 1 here because in the C++ code it goes out of bounds by 1 byte at max.
            _alphaSelectors = new ushort[_header.AlphaSelectors.Count * 6 + 1];
            var s_linear = new byte[8];
            int data_pos = 0;
            int i = 0;
            // - 1 because we added one before.
            while (i < _header.AlphaSelectors.Count - 1)
            {
                uint s_group = 0;
                for (int p = 0; p < 16; p++)
                {
                    if ((p & 1) == 1)
                    {
                        s_group >>= 3;
                    }
                    else
                    {
                        s_linear[p >> 1] ^= (byte)_codec.Decode(dm);
                        s_group = s_linear[p >> 1];
                    }
                    byte s = (byte)(s_group & 7);
                    if (s <= 3)
                    {
                        s = (byte)(3 - s);
                    }
                    int d = 3 * (p + 1);
                    int byte_offset = d >> 3;
                    int bit_offset = d & 7;
                    WriteByteToUInt16(
                        _alphaSelectors,
                        data_pos + byte_offset,
                        (ushort)(((s << (8 - bit_offset)) & 0xFF))
                    );
                    if (bit_offset < 3)
                    {
                        WriteByteToUInt16(
                            _alphaSelectors,
                            data_pos + byte_offset - 1,
                            (ushort)(s >> bit_offset)
                        );
                    }
                    d += 9 * ((p & 3) - (p >> 2));
                    byte_offset = d >> 3;
                    bit_offset = d & 7;
                    WriteByteToUInt16(
                        _alphaSelectors,
                        data_pos + byte_offset + 6,
                        (ushort)(((s << (8 - bit_offset)) & 0xFF))
                    );
                    if (bit_offset < 3)
                    {
                        WriteByteToUInt16(
                            _alphaSelectors,
                            data_pos + byte_offset + 5,
                            (ushort)(s >> bit_offset)
                        );
                    }
                }
                i += 6;
                data_pos += 12;
            }
            return true;
        }

        private static void WriteByteToUInt16(ushort[] buffer, int index, ushort val)
        {
            var half = index >> 1;
            var next = new byte[2];
            BinaryPrimitives.WriteUInt16BigEndian(next, buffer[half]);
            if ((index & 1) != 1)
            {
                buffer[half] = (ushort)((ushort)next[0] << 8 | next[1] | val);
            } else
            {
                buffer[half] = (ushort)(next[0] | val << 8 | next[1]);
            }
        }

        public bool DecodeAlphaSelectorsEtcs()
        {
            bool res = _codec.StartDecoding(
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
            _alphaSelectors = new ushort[(_header.AlphaSelectors.Count * 3) + 1];
            byte[] s_linear = new byte[8];
            int i = 0;
            while (i < ((_alphaSelectors.Length - 1) << 1))
            {
                uint s_group = 0;
                for (int p = 0; p < 16; p++)
                {
                    if ((p & 1) == 1)
                    {
                        s_group >>= 3;
                    }
                    else
                    {
                        s_linear[p >> 1] ^= (byte)_codec.Decode(dm);
                        s_group = s_linear[p >> 1];
                    }
                    byte s = (byte)(s_group & 7);
                    if (s <= 3)
                    {
                        s = (byte)(3 - s);
                    }
                    short d = (short)(3 * (p + 1) + 9 * ((p & 3) - (p >> 2)));
                    int byte_offset = d >> 3;
                    int bit_offset = d & 7;
                    WriteByteToUInt16(
                        _alphaSelectors,
                        i + byte_offset,
                        (ushort)(((s << (8 - bit_offset)) & 0xFF))
                    );
                    if (bit_offset < 3)
                    {
                        WriteByteToUInt16(
                            _alphaSelectors,
                            i + byte_offset - 1,
                            (ushort)(s >> bit_offset)
                        );
                    }
                }
                i += 6;
            }
            return true;
        }


        protected override void Unpack(Span<byte> output, int rowPitchInBytes, int blocksWidth, int blocksHeight)
        {
            switch (_header.Format)
            {
                case CrunchFormat.Dxt1:
                case CrunchFormat.Etc1s:
                    UnpackDxt1(output, rowPitchInBytes, blocksWidth, blocksHeight);
                    break;

                case CrunchFormat.CCrnfmtDxt5:
                case CrunchFormat.Dxt5CcxY:
                case CrunchFormat.Dxt5XGbr:
                case CrunchFormat.Dxt5Agbr:
                case CrunchFormat.Dxt5XGxR:
                case CrunchFormat.Etc2as:
                    UnpackDxt5(output, rowPitchInBytes, blocksWidth, blocksHeight);
                    break;

                case CrunchFormat.Dxt5a:
                    UnpackDxt5a(output, rowPitchInBytes, blocksWidth, blocksHeight);
                    break;

                case CrunchFormat.DxnXy:
                case CrunchFormat.DxnYx:
                    UnpackDxn(output, rowPitchInBytes, blocksWidth, blocksHeight);
                    break;
                case CrunchFormat.Etc1:
                case CrunchFormat.Etc2:
                    UnpackEtc1(output, rowPitchInBytes, blocksWidth, blocksHeight);
                    break;

                case CrunchFormat.Etc2a:
                    UnpackEtc2a(output, rowPitchInBytes, blocksWidth, blocksHeight);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private bool UnpackDxt1(
            Span<byte> output,
            int outputPitchInBytes,
            int outputWidth,
            int outputHeight)
        {
            uint numColorEndpoints = (uint)_colorEndpoints.Length;
            int width = (outputWidth + 1) & ~1;
            int height = (outputHeight + 1) & ~1;
            int deltaPitchInDwords = ((outputPitchInBytes >> 2) - (width << 1));
            var blockBuffer = new BlockBufferElement[width];
            for (int i = 0; i < blockBuffer.Length; i++)
            {
                blockBuffer[i] = new();
            }
            int colorEndpointIndex = 0;
            byte referenceGroup = 0;
            int dataPos = 0;
            for (int f = 0; f < _header.Faces; f++)
            {
                for (uint y = 0; y < height; y++)
                {
                    bool visible = y < outputHeight;
                    for (int x = 0; x < width; x++)
                    {
                        visible = visible && x < outputWidth;
                        if ((y & 1) == 0 && (x & 1) == 0)
                        {
                            referenceGroup = (byte)_codec.Decode(_chunkEncodingDm);
                        }
                        var buffer = blockBuffer[x];
                        byte endpointReference;
                        if ((y & 1) == 1)
                        {
                            endpointReference = (byte)buffer.EndpointReference;
                        }
                        else
                        {
                            endpointReference = (byte)(referenceGroup & 3);
                            referenceGroup >>= 2;
                            buffer.EndpointReference = (ushort)(referenceGroup & 3);
                            referenceGroup >>= 2;
                        }
                        if (endpointReference == 0)
                        {
                            colorEndpointIndex += (int)_codec.Decode(_endpointDeltaDm[0]);
                            if (colorEndpointIndex >= numColorEndpoints)
                            {
                                colorEndpointIndex -= (int)numColorEndpoints;
                            }
                            buffer.ColorEndpointIndex = (ushort)colorEndpointIndex;
                        }
                        else if (endpointReference == 1)
                        {
                            buffer.ColorEndpointIndex = (ushort)colorEndpointIndex;
                        }
                        else
                        {
                            colorEndpointIndex = (int)buffer.ColorEndpointIndex;
                        }
                        int colorSelectorIndex = (int)_codec.Decode(_selectorDeltaDm[0]);
                        if (visible)
                        {
                            WriteUInt(output, dataPos, _colorEndpoints[colorEndpointIndex]);
                            WriteUInt(output, dataPos + 1, _colorSelectors[colorSelectorIndex]);
                        }
                        dataPos += 2;
                    }
                    dataPos += deltaPitchInDwords;
                }
            }
            return true;
        }

        private bool UnpackDxt5(
            Span<byte> output,
            int outputPitchInBytes,
            int outputWidth,
            int outputHeight)
        {
            uint numColorEndpoints = (uint)_colorEndpoints.Length;
            uint numAlphaEndpoints = (uint)_alphaEndpoints.Length;
            int width = (outputWidth + 1) & ~1;
            int height = (outputHeight + 1) & ~1;
            int deltaPitchInDwords = ((outputPitchInBytes >> 2) - (width << 2));
            var blockBuffer = new BlockBufferElement[width];
            for (int i = 0; i < blockBuffer.Length; i++)
            {
                blockBuffer[i] = new();
            }
            int colorEndpointIndex = 0;
            int alpha0EndpointIndex = 0;
            byte referenceGroup = 0;
            int dataPos = 0;
            for (int f = 0; f < _header.Faces; f++)
            {
                for (uint y = 0; y < height; y++)
                {
                    bool visible = y < outputHeight;
                    for (int x = 0; x < width; x++)
                    {
                        visible = visible && x < outputWidth;
                        if ((y & 1) == 0 && (x & 1) == 0)
                        {
                            referenceGroup = (byte)_codec.Decode(_chunkEncodingDm);
                        }
                        var buffer = blockBuffer[x];
                        byte endpointReference;
                        if ((y & 1) == 1)
                        {
                            endpointReference = (byte)buffer.EndpointReference;
                        }
                        else
                        {
                            endpointReference = (byte)(referenceGroup & 3);
                            referenceGroup >>= 2;
                            buffer.EndpointReference = (ushort)(referenceGroup & 3);
                            referenceGroup >>= 2;
                        }
                        if (endpointReference == 0)
                        {
                            colorEndpointIndex += (int)_codec.Decode(_endpointDeltaDm[0]);
                            if (colorEndpointIndex >= numColorEndpoints)
                            {
                                colorEndpointIndex -= (int)numColorEndpoints;
                            }
                            buffer.ColorEndpointIndex = (ushort)colorEndpointIndex;

                            alpha0EndpointIndex += (int)_codec.Decode(_endpointDeltaDm[1]);
                            if (alpha0EndpointIndex >= numAlphaEndpoints)
                            {
                                alpha0EndpointIndex -= (int)numAlphaEndpoints;
                            }
                            buffer.Alpha0EndpointIndex = (ushort)alpha0EndpointIndex;
                        }
                        else if (endpointReference == 1)
                        {
                            buffer.ColorEndpointIndex = (ushort)colorEndpointIndex;
                            buffer.Alpha0EndpointIndex = (ushort)alpha0EndpointIndex;
                        }
                        else
                        {
                            colorEndpointIndex = buffer.ColorEndpointIndex;
                            alpha0EndpointIndex = buffer.Alpha0EndpointIndex;
                        }
                        int colorSelectorIndex = (int)_codec.Decode(_selectorDeltaDm[0]);
                        int alpha0SelectorIndex = (int)_codec.Decode(_selectorDeltaDm[1]);
                        if (visible)
                        {
                            var pAlpha0Selectors = _alphaSelectors.Skip(alpha0SelectorIndex * 3).ToArray();
                            WriteUInt(output, dataPos, _alphaEndpoints[alpha0EndpointIndex] | ((uint)pAlpha0Selectors[0] << 16));
                            WriteUInt(output, dataPos + 1, pAlpha0Selectors[1] | ((uint)pAlpha0Selectors[2] << 16));
                            WriteUInt(output, dataPos + 2, _colorEndpoints[colorEndpointIndex]);
                            WriteUInt(output, dataPos + 3, _colorSelectors[colorSelectorIndex]);
                        }
                        dataPos += 4;
                    }
                    dataPos += deltaPitchInDwords;
                }
            }
            return true;
        }

        private bool UnpackDxt5a(
            Span<byte> output,
            int outputPitchInBytes,
            int outputWidth,
            int outputHeight)
        {
            uint numAlphaEndpoints = (uint)_alphaEndpoints.Length;
            int width = (outputWidth + 1) & ~1;
            int height = (outputHeight + 1) & ~1;
            int deltaPitchInDwords = ((outputPitchInBytes >> 2) - (width << 1));
            var blockBuffer = new BlockBufferElement[width];
            for (int i = 0; i < blockBuffer.Length; i++)
            {
                blockBuffer[i] = new();
            }
            int alpha0EndpointIndex = 0;
            byte referenceGroup = 0;
            int dataPos = 0;
            for (int f = 0; f < _header.Faces; f++)
            {
                for (uint y = 0; y < height; y++)
                {
                    bool visible = y < outputHeight;
                    for (int x = 0; x < width; x++)
                    {
                        visible = visible && x < outputWidth;
                        if ((y & 1) == 0 && (x & 1) == 0)
                        {
                            referenceGroup = (byte)_codec.Decode(_chunkEncodingDm);
                        }
                        var buffer = blockBuffer[x];
                        byte endpointReference;
                        if ((y & 1) == 1)
                        {
                            endpointReference = (byte)buffer.EndpointReference;
                        }
                        else
                        {
                            endpointReference = (byte)(referenceGroup & 3);
                            referenceGroup >>= 2;
                            buffer.EndpointReference = (ushort)(referenceGroup & 3);
                            referenceGroup >>= 2;
                        }
                        if (endpointReference == 0)
                        {
                            alpha0EndpointIndex += (int)_codec.Decode(_endpointDeltaDm[1]);
                            if (alpha0EndpointIndex >= numAlphaEndpoints)
                            {
                                alpha0EndpointIndex -= (int)numAlphaEndpoints;
                            }
                            buffer.Alpha0EndpointIndex = (ushort)alpha0EndpointIndex;
                        }
                        else if (endpointReference == 1)
                        {
                            buffer.Alpha0EndpointIndex = (ushort)alpha0EndpointIndex;
                        }
                        else
                        {
                            alpha0EndpointIndex = buffer.Alpha0EndpointIndex;
                        }
                        int alpha0SelectorIndex = (int)_codec.Decode(_selectorDeltaDm[1]);
                        if (visible)
                        {
                            var pAlpha0Selectors = _alphaSelectors.Skip(alpha0SelectorIndex * 3).ToArray();
                            WriteUInt(output, dataPos, _alphaEndpoints[alpha0EndpointIndex] | ((uint)pAlpha0Selectors[0] << 16));
                            WriteUInt(output, dataPos + 1, pAlpha0Selectors[1] | ((uint)pAlpha0Selectors[2] << 16));
                        }
                        dataPos += 2;
                    }
                    dataPos += deltaPitchInDwords;
                }
            }
            return true;
        }

        private bool UnpackDxn(
                Span<byte> output,
                int outputPitchInBytes,
                int outputWidth,
                int outputHeight)
        {
            uint numAlphaEndpoints = (uint)_alphaEndpoints.Length;
            int width = (outputWidth + 1) & ~1;
            int height = (outputHeight + 1) & ~1;
            int deltaPitchInDwords = ((outputPitchInBytes >> 2) - (width << 2));
            var blockBuffer = new BlockBufferElement[width];
            for (int i = 0; i < blockBuffer.Length; i++)
            {
                blockBuffer[i] = new();
            }
            int alpha0EndpointIndex = 0;
            int alpha1EndpointIndex = 0;
            byte referenceGroup = 0;
            int dataPos = 0;
            for (int f = 0; f < _header.Faces; f++)
            {
                for (uint y = 0; y < height; y++)
                {
                    bool visible = y < outputHeight;
                    for (int x = 0; x < width; x++)
                    {
                        visible = visible && x < outputWidth;
                        if ((y & 1) == 0 && (x & 1) == 0)
                        {
                            referenceGroup = (byte)_codec.Decode(_chunkEncodingDm);
                        }
                        var buffer = blockBuffer[x];
                        byte endpointReference;
                        if ((y & 1) == 1)
                        {
                            endpointReference = (byte)buffer.EndpointReference;
                        }
                        else
                        {
                            endpointReference = (byte)(referenceGroup & 3);
                            referenceGroup >>= 2;
                            buffer.EndpointReference = (ushort)(referenceGroup & 3);
                            referenceGroup >>= 2;
                        }
                        if (endpointReference == 0)
                        {
                            alpha0EndpointIndex += (int)_codec.Decode(_endpointDeltaDm[1]);
                            if (alpha0EndpointIndex >= numAlphaEndpoints)
                            {
                                alpha0EndpointIndex -= (int)numAlphaEndpoints;
                            }
                            buffer.Alpha0EndpointIndex = (ushort)alpha0EndpointIndex;

                            alpha1EndpointIndex += (int)_codec.Decode(_endpointDeltaDm[1]);
                            if (alpha1EndpointIndex >= numAlphaEndpoints)
                            {
                                alpha1EndpointIndex -= (int)numAlphaEndpoints;
                            }
                            buffer.Alpha1EndpointIndex = (ushort)alpha1EndpointIndex;
                        }
                        else if (endpointReference == 1)
                        {
                            buffer.Alpha0EndpointIndex = (ushort)alpha0EndpointIndex;
                            buffer.Alpha1EndpointIndex = (ushort)alpha1EndpointIndex;
                        }
                        else
                        {
                            alpha0EndpointIndex = buffer.Alpha0EndpointIndex;
                            alpha1EndpointIndex = buffer.Alpha1EndpointIndex;
                        }
                        int alpha0SelectorIndex = (int)_codec.Decode(_selectorDeltaDm[1]);
                        int alpha1SelectorIndex = (int)_codec.Decode(_selectorDeltaDm[1]);
                        if (visible)
                        {
                            var pAlpha0Selectors = _alphaSelectors.Skip(alpha0SelectorIndex * 3).ToArray();
                            var pAlpha1Selectors = _alphaSelectors.Skip(alpha1SelectorIndex * 3).ToArray();
                            WriteUInt(output, dataPos, _alphaEndpoints[alpha0EndpointIndex] | ((uint)pAlpha0Selectors[0] << 16));
                            WriteUInt(output, dataPos + 1, pAlpha0Selectors[1] | ((uint)pAlpha0Selectors[2] << 16));
                            WriteUInt(output, dataPos + 2, _alphaEndpoints[alpha1EndpointIndex] | ((uint)pAlpha1Selectors[0] << 16));
                            WriteUInt(output, dataPos + 3, pAlpha1Selectors[1] | ((uint)pAlpha1Selectors[2] << 16));
                        }
                        dataPos += 4;
                    }
                    dataPos += deltaPitchInDwords;
                }
            }
            return true;
        }

        private bool UnpackEtc1(
            Span<byte> output,
            int outputPitchInBytes,
            int outputWidth,
            int outputHeight)
        {
            uint numColorEndpoints = (uint)_colorEndpoints.Length;
            int width = (outputWidth + 1) & ~1;
            int height = (outputHeight + 1) & ~1;
            int deltaPitchInDwords = (outputPitchInBytes >> 2) - (width << 1);
            var blockBuffer = new BlockBufferElement[width << 1];
            for (int i = 0; i < blockBuffer.Length; i++)
            {
                blockBuffer[i] = new();
            }
            int colorEndpointIndex = 0;
            int diagonalColorEndpointIndex = 0;
            byte referenceGroup;
            int dataPos = 0;
            for (int f = 0; f < _header.Faces; f++)
            {
                for (uint y = 0; y < height; y++)
                {
                    bool visible = y < outputHeight;
                    for (int x = 0; x < width; x++)
                    {
                        visible = visible && x < outputWidth;
                        var buffer = blockBuffer[x << 1];
                        byte endpointReference;
                        byte[] blockEndpoint = new byte[4];
                        if (y % 2 == 1)
                        {
                            endpointReference = (byte)buffer.EndpointReference;
                        }
                        else
                        {
                            referenceGroup = (byte)_codec.Decode(_chunkEncodingDm);
                            endpointReference = (byte)((referenceGroup & 3) | ((referenceGroup >> 2) & 12));
                            buffer.EndpointReference = (ushort)((referenceGroup >> 2 & 3) | ((referenceGroup >> 4) & 12));
                        }
                        if ((endpointReference & 3) == 0)
                        {
                            colorEndpointIndex += (int)_codec.Decode(_endpointDeltaDm[0]);
                            if (colorEndpointIndex >= numColorEndpoints)
                            {
                                colorEndpointIndex -= (int)numColorEndpoints;
                            }
                            buffer.ColorEndpointIndex = (ushort)colorEndpointIndex;
                        }
                        else if ((endpointReference & 3) == 1)
                        {
                            buffer.ColorEndpointIndex = (ushort)colorEndpointIndex;
                        }
                        else if ((endpointReference & 3) == 3)
                        {
                            colorEndpointIndex = diagonalColorEndpointIndex;
                            buffer.ColorEndpointIndex = (ushort)colorEndpointIndex;
                        }
                        else
                        {
                            colorEndpointIndex = buffer.ColorEndpointIndex;
                        }
                        endpointReference >>= 2;
                        byte[] e0 = BitConverter.GetBytes(_colorEndpoints[colorEndpointIndex]);
                        int selectorIndex = (int)_codec.Decode(_selectorDeltaDm[0]);
                        if (endpointReference != 0)
                        {
                            colorEndpointIndex += (int)_codec.Decode(_endpointDeltaDm[0]);
                            if (colorEndpointIndex >= numColorEndpoints)
                            {
                                colorEndpointIndex -= (int)numColorEndpoints;
                            }
                        }
                        diagonalColorEndpointIndex = blockBuffer[(x << 1) | 1].ColorEndpointIndex;
                        blockBuffer[(x << 1) | 1].ColorEndpointIndex = (ushort)colorEndpointIndex;
                        var e1 = BitConverter.GetBytes(_colorEndpoints[colorEndpointIndex]);
                        if (visible)
                        {
                            byte flip = (byte)((endpointReference >> 1) ^ 1);
                            byte diff = 1;
                            for (int c = 0; c < 3; c++)
                            {
                                if (diff == 0)
                                {
                                    break;
                                }
                                if (!(e0[c] + 3 >= e1[c] && e1[c] + 4 >= e0[c]))
                                {
                                    diff = 0;
                                }
                            }
                            for (int c = 0; c < 3; c++)
                            {
                                if (diff != 0)
                                {
                                    blockEndpoint[c] = (byte)(e0[c] << 3 | ((e1[c] - e0[c]) & 7));
                                }
                                else
                                {
                                    blockEndpoint[c] = (byte)((e0[c] << 3 & 0xF0) | (e1[c] >> 1));
                                }
                            }
                            blockEndpoint[3] = (byte)(e0[3] << 5 | e1[3] << 2 | (diff << 1) | flip);
                            output[dataPos * 4] = blockEndpoint[0];
                            output[dataPos * 4 + 1] = blockEndpoint[1];
                            output[dataPos * 4 + 2] = blockEndpoint[2];
                            output[dataPos * 4 + 3] = blockEndpoint[3];
                            WriteUInt(output, dataPos + 1, _colorSelectors[(selectorIndex << 1) | flip]);
                        }
                        dataPos += 2;
                    }
                    dataPos += deltaPitchInDwords;
                }
            }
            return true;
        }

        private bool UnpackEtc2a(
            Span<byte> output,
            int outputPitchInBytes,
            int outputWidth,
            int outputHeight)
        {
            uint numColorEndpoints = (uint)_colorEndpoints.Length;
            uint numAlphaEndpoints = (uint)_alphaEndpoints.Length;
            int width = (outputWidth + 1) & ~1;
            int height = (outputHeight + 1) & ~1;
            int deltaPitchInDwords = ((outputPitchInBytes >> 2) - (width << 2));
            var blockBuffer = new BlockBufferElement[width << 1];
            for (int i = 0; i < blockBuffer.Length; i++)
            {
                blockBuffer[i] = new();
            }
            int colorEndpointIndex = 0;
            int alpha0EndpointIndex = 0;
            int diagonalColorEndpointIndex = 0;
            int diagonalAlpha0EndpointIndex = 0;
            byte referenceGroup;
            int dataPos = 0;
            for (int f = 0; f < _header.Faces; f++)
            {
                for (uint y = 0; y < height; y++)
                {
                    bool visible = y < outputHeight;
                    for (int x = 0; x < width; x++)
                    {
                        visible = visible && x < outputWidth;
                        var buffer = blockBuffer[x << 1];
                        byte endpointReference;
                        byte[] blockEndpoint = new byte[4];
                        if (y % 2 == 1)
                        {
                            endpointReference = (byte)buffer.EndpointReference;
                        }
                        else
                        {
                            referenceGroup = (byte)(_codec.Decode(_chunkEncodingDm));
                            endpointReference = (byte)((referenceGroup & 3) | ((referenceGroup >> 2) & 12));
                            buffer.EndpointReference = (ushort)(((referenceGroup >> 2) & 3) | ((referenceGroup >> 4) & 12));
                        }
                        if ((endpointReference & 3) == 0)
                        {
                            colorEndpointIndex += (int)_codec.Decode(_endpointDeltaDm[0]);
                            if (colorEndpointIndex >= numColorEndpoints)
                            {
                                colorEndpointIndex -= (int)numColorEndpoints;
                            }
                            alpha0EndpointIndex += (int)_codec.Decode(_endpointDeltaDm[1]);
                            if (alpha0EndpointIndex >= numAlphaEndpoints)
                            {
                                alpha0EndpointIndex -= (int)numAlphaEndpoints;
                            }
                            buffer.ColorEndpointIndex = (ushort)colorEndpointIndex;
                            buffer.Alpha0EndpointIndex = (ushort)alpha0EndpointIndex;
                        }
                        else if ((endpointReference & 3) == 1)
                        {
                            buffer.ColorEndpointIndex = (ushort)colorEndpointIndex;
                            buffer.Alpha0EndpointIndex = (ushort)alpha0EndpointIndex;
                        }
                        else if ((endpointReference & 3) == 3)
                        {
                            colorEndpointIndex = diagonalColorEndpointIndex;
                            buffer.ColorEndpointIndex = (ushort)colorEndpointIndex;
                            alpha0EndpointIndex = diagonalAlpha0EndpointIndex;
                            buffer.Alpha0EndpointIndex = (ushort)alpha0EndpointIndex;
                        }
                        else
                        {
                            colorEndpointIndex = buffer.ColorEndpointIndex;
                            alpha0EndpointIndex = buffer.Alpha0EndpointIndex;
                        }
                        endpointReference >>= 2;
                        byte[] e0 = BitConverter.GetBytes(_colorEndpoints[colorEndpointIndex]);
                        int colorSelectorIndex = (int)_codec.Decode(_selectorDeltaDm[0]);
                        int alpha0SelectorIndex = (int)_codec.Decode(_selectorDeltaDm[1]);
                        if (endpointReference != 0)
                        {
                            colorEndpointIndex += (int)_codec.Decode(_endpointDeltaDm[0]);
                            if (colorEndpointIndex >= numColorEndpoints)
                            {
                                colorEndpointIndex -= (int)numColorEndpoints;
                            }
                        }
                        byte[] e1 = BitConverter.GetBytes(_colorEndpoints[colorEndpointIndex]);
                        diagonalColorEndpointIndex = (int)blockBuffer[(x << 1) | 1].ColorEndpointIndex;
                        diagonalAlpha0EndpointIndex = (int)blockBuffer[(x << 1) | 1].Alpha0EndpointIndex;
                        blockBuffer[(x << 1) | 1].ColorEndpointIndex = (ushort)colorEndpointIndex;
                        blockBuffer[(x << 1) | 1].Alpha0EndpointIndex = (ushort)alpha0EndpointIndex;
                        if (visible)
                        {
                            byte flip = (byte)((endpointReference >> 1) ^ 1);
                            byte diff = 1;
                            for (int c = 0; c < 3; c++)
                            {
                                if (diff == 0)
                                {
                                    break;
                                }
                                if (!(e0[c] + 3 >= e1[c] && e1[c] + 4 >= e0[c]))
                                {
                                    diff = 0;
                                }
                            }
                            for (int c = 0; c < 3; c++)
                            {
                                if (diff != 0)
                                {
                                    blockEndpoint[c] = (byte)(e0[c] << 3 | ((e1[c] - e0[c]) & 7));
                                }
                                else
                                {
                                    blockEndpoint[c] = (byte)((e0[c] << 3 & 0xF0) | (e1[c] >> 1));
                                }
                            }
                            blockEndpoint[3] = (byte)(e0[3] << 5 | e1[3] << 2 | (diff << 1) | flip);
                            ushort[] pAlpha0Selectors = (flip != 0) ?
                                _alphaSelectors.Skip(alpha0SelectorIndex * 6 + 3).ToArray() :
                                _alphaSelectors.Skip(alpha0SelectorIndex * 6).ToArray();
                            WriteUInt(output, dataPos, _alphaEndpoints[alpha0EndpointIndex] | ((uint)pAlpha0Selectors[0] << 16));
                            WriteUInt(output, dataPos + 1, pAlpha0Selectors[1] | ((uint)pAlpha0Selectors[2] << 16));
                            output[(dataPos + 2) * 4] = blockEndpoint[0];
                            output[(dataPos + 2) * 4 + 1] = blockEndpoint[1];
                            output[(dataPos + 2) * 4 + 2] = blockEndpoint[2];
                            output[(dataPos + 2) * 4 + 3] = blockEndpoint[3];
                            WriteUInt(output, dataPos + 3, _colorSelectors[(colorSelectorIndex << 1) | flip]);
                        }
                        dataPos += 4;
                    }
                    dataPos += deltaPitchInDwords;
                }
            }
            return true;
        }

        #endregion

        protected override int GetBlockSize(CrunchFormat format)
        {
            return format is CrunchFormat.Dxt1 or CrunchFormat.Dxt5a
                or CrunchFormat.Etc1 or CrunchFormat.Etc2 or CrunchFormat.Etc1s ? 8 : 16;
        }

        protected override int GetBitsPerTexel(CrunchFormat format)
        {
            return format switch
            {
                CrunchFormat.Dxt1 or CrunchFormat.Dxt5a or CrunchFormat.Etc1
                    or CrunchFormat.Etc2
                    or CrunchFormat.Etc1s => 4,

                CrunchFormat.Dxt3
                    or CrunchFormat.CCrnfmtDxt5
                    or CrunchFormat.DxnXy
                    or CrunchFormat.DxnYx
                    or CrunchFormat.Dxt5CcxY
                    or CrunchFormat.Dxt5XGxR
                    or CrunchFormat.Dxt5XGbr
                    or CrunchFormat.Dxt5Agbr
                    or CrunchFormat.Etc2a
                    or CrunchFormat.Etc2as => 8,
                _ => throw new NotSupportedException(format.ToString()),
            };
        }
    }
}
