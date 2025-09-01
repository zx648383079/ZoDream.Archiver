using System;
using System.Buffers.Binary;
using System.Numerics;

namespace ZoDream.Shared.Drawing
{
    public class BC7 : BlockBufferDecoder
    {
        protected override int BlockSize => 16;
        protected override void DecodeBlock(ReadOnlySpan<byte> data, Span<byte> output)
        {
            BitStream bstream = new(data);
            SizeSpan<uint> endpoints = new(stackalloc uint[6 * 4], 6, 4);
            SizeSpan<int> indices = new(stackalloc int[4 * 4], 4, 4);

            int mode = ReadBc7Mode(ref bstream);

            // unexpected mode, clear the block (transparent black)
            if (mode >= 8)
            {
                output.Clear();

                return;
            }

            uint partition = 0;
            int numPartitions = 1;
            uint rotation = 0;
            uint indexSelectionBit = 0;

            switch (mode)
            {
                case 0:
                    numPartitions = 3;
                    partition = bstream.ReadBits(4);
                    break;
                case 1 or 3 or 7:
                    numPartitions = 2;
                    partition = bstream.ReadBits(6);
                    break;
                case 2:
                    numPartitions = 3;
                    partition = bstream.ReadBits(6);
                    break;
                case 4:
                    rotation = bstream.ReadBits(2);
                    indexSelectionBit = bstream.ReadBit();
                    break;
                case 5:
                    rotation = bstream.ReadBits(2);
                    break;
            }

            int numEndpoints = numPartitions * 2;

            // Extract endpoints 
            // RGB 
            for (int i = 0; i < 3; ++i)
            {
                for (int j = 0; j < numEndpoints; ++j)
                {
                    endpoints[j, i] = bstream.ReadBits(ActualBitsCountTable[0][mode]);
                }
            }
            // Alpha (if any) 
            if (ActualBitsCountTable[1][mode] > 0)
            {
                for (int j = 0; j < numEndpoints; ++j)
                {
                    endpoints[j, 3] = bstream.ReadBits(ActualBitsCountTable[1][mode]);
                }
            }

            // Fully decode endpoints 
            // First handle modes that have P-bits 
            if (mode == 0 || mode == 1 || mode == 3 || mode == 6 || mode == 7)
            {
                for (int i = 0; i < numEndpoints; ++i)
                {
                    // component-wise left-shift 
                    for (int j = 0; j < 4; ++j)
                    {
                        endpoints[i, j] <<= 1;
                    }
                }

                // if P-bit is shared 
                if (mode == 1)
                {
                    uint i = bstream.ReadBit();
                    uint j = bstream.ReadBit();

                    // rgb component-wise insert pbits 
                    for (int k = 0; k < 3; ++k)
                    {
                        endpoints[0, k] |= i;
                        endpoints[1, k] |= i;
                        endpoints[2, k] |= j;
                        endpoints[3, k] |= j;
                    }
                }
                else if ((bcdec_bc7_sModeHasPBits & (1 << mode)) != 0)
                {
                    // unique P-bit per endpoint 
                    for (int i = 0; i < numEndpoints; ++i)
                    {
                        uint j = bstream.ReadBit();
                        for (int k = 0; k < 4; ++k)
                        {
                            endpoints[i, k] |= j;
                        }
                    }
                }
            }

            for (int i = 0; i < numEndpoints; ++i)
            {
                // get color components precision including pbit 
                int j = ActualBitsCountTable[0][mode] + ((bcdec_bc7_sModeHasPBits >> mode) & 1);

                for (int k = 0; k < 3; ++k)
                {
                    // left shift endpoint components so that their MSB lies in bit 7 
                    endpoints[i, k] = endpoints[i, k] << (8 - j);
                    // Replicate each component's MSB into the LSBs revealed by the left-shift operation above 
                    endpoints[i, k] = endpoints[i, k] | (endpoints[i, k] >> j);
                }

                // get alpha component precision including pbit 
                j = ActualBitsCountTable[1][mode] + ((bcdec_bc7_sModeHasPBits >> mode) & 1);

                // left shift endpoint components so that their MSB lies in bit 7 
                endpoints[i, 3] = endpoints[i, 3] << (8 - j);
                // Replicate each component's MSB into the LSBs revealed by the left-shift operation above 
                endpoints[i, 3] = endpoints[i, 3] | (endpoints[i, 3] >> j);
            }

            // If this mode does not explicitly define the alpha component 
            // set alpha equal to 1.0 
            if (ActualBitsCountTable[1][mode] == 0)
            {
                for (int j = 0; j < numEndpoints; ++j)
                {
                    endpoints[j, 3] = 0xFF;
                }
            }

            // Determine weights tables 
            int indexBits = mode switch
            {
                0 or 1 => 3,
                6 => 4,
                _ => 2
            };
            int indexBits2 = mode switch
            {
                4 => 3,
                5 => 2,
                _ => 0
            };
            ReadOnlySpan<int> weights = indexBits switch
            {
                2 => AWeight2Table,
                3 => AWeight3Table,
                _ => AWeight4Table
            };
            ReadOnlySpan<int> weights2 = (indexBits2 == 2) ? AWeight2Table : AWeight3Table;

            // Quite inconvenient that indices aren't interleaved so we have to make 2 passes here 
            // Pass #1: collecting color indices 
            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    int partitionSet = (numPartitions == 1) ? ((i | j) != 0 ? 0 : 128) : PartitionSetsTable[numPartitions - 2][partition][i][j];

                    indexBits = (mode == 0 || mode == 1) ? 3 : ((mode == 6) ? 4 : 2);
                    // fix-up index is specified with one less bit 
                    // The fix-up index for subset 0 is always index 0 
                    if ((partitionSet & 0x80) != 0)
                    {
                        indexBits--;
                    }

                    indices[i, j] = (int)bstream.ReadBits(indexBits);
                }
            }

            // Pass #2: reading alpha indices (if any) and interpolating & rotating 
            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    int partitionSet = (numPartitions == 1)
                        ? ((i | j) != 0 ? 0 : 128)
                        : PartitionSetsTable[numPartitions - 2][partition][i][j];
                    partitionSet &= 0x03;

                    int index = indices[i, j];

                    uint r;
                    uint g;
                    uint b;
                    uint a;

                    if (indexBits2 == 0)
                    {
                        r = BC6h.Interpolate(endpoints[partitionSet * 2, 0], endpoints[(partitionSet * 2) + 1, 0], weights, index);
                        g = BC6h.Interpolate(endpoints[partitionSet * 2, 1], endpoints[(partitionSet * 2) + 1, 1], weights, index);
                        b = BC6h.Interpolate(endpoints[partitionSet * 2, 2], endpoints[(partitionSet * 2) + 1, 2], weights, index);
                        a = BC6h.Interpolate(endpoints[partitionSet * 2, 3], endpoints[(partitionSet * 2) + 1, 3], weights, index);
                    }
                    else
                    {
                        int index2 = (int)bstream.ReadBits((i | j) != 0 ? indexBits2 : (indexBits2 - 1));
                        // The index value for interpolating color comes from the secondary index bits for the texel
                        // if the mode has an index selection bit and its value is one, and from the primary index bits otherwise.
                        // The alpha index comes from the secondary index bits if the block has a secondary index
                        // and the block either doesn't have an index selection bit or that bit is zero,
                        // and from the primary index bits otherwise.
                        if (indexSelectionBit == 0)
                        {
                            r = BC6h.Interpolate(endpoints[partitionSet * 2, 0], endpoints[(partitionSet * 2) + 1, 0], weights, index);
                            g = BC6h.Interpolate(endpoints[partitionSet * 2, 1], endpoints[(partitionSet * 2) + 1, 1], weights, index);
                            b = BC6h.Interpolate(endpoints[partitionSet * 2, 2], endpoints[(partitionSet * 2) + 1, 2], weights, index);
                            a = BC6h.Interpolate(endpoints[partitionSet * 2, 3], endpoints[(partitionSet * 2) + 1, 3], weights2, index2);
                        }
                        else
                        {
                            r = BC6h.Interpolate(endpoints[partitionSet * 2, 0], endpoints[(partitionSet * 2) + 1, 0], weights2, index2);
                            g = BC6h.Interpolate(endpoints[partitionSet * 2, 1], endpoints[(partitionSet * 2) + 1, 1], weights2, index2);
                            b = BC6h.Interpolate(endpoints[partitionSet * 2, 2], endpoints[(partitionSet * 2) + 1, 2], weights2, index2);
                            a = BC6h.Interpolate(endpoints[partitionSet * 2, 3], endpoints[(partitionSet * 2) + 1, 3], weights, index);
                        }
                    }

                    switch (rotation)
                    {
                        case 1:
                            {
                                // 01 – Block format is Scalar(R) Vector(AGB) - swap A and R
                                (a, r) = (r, a);
                            }
                            break;
                        case 2:
                            {
                                // 10 – Block format is Scalar(G) Vector(RAB) - swap A and G
                                (a, g) = (g, a);
                            }
                            break;
                        case 3:
                            {
                                // 11 - Block format is Scalar(B) Vector(RGA) - swap A and B
                                (a, b) = (b, a);
                            }
                            break;
                    }

                    int offset = (i * 16) + (j * 4);
                    output[offset + 0] = (byte)r;
                    output[offset + 1] = (byte)g;
                    output[offset + 2] = (byte)b;
                    output[offset + 3] = (byte)a;
                }
            }

            
        }

        private static int ReadBc7Mode(ref BitStream bstream)
        {
            // Read bits until we find a non-zero bit, up to 8 bits.
            // The mode is the number of bits read.
            uint eightBits = bstream.PeakBits(8);
            int zeros = BitOperations.TrailingZeroCount(eightBits);
            int bitCount = zeros >= 8 ? 8 : zeros + 1;
            bstream.Advance(bitCount);
            return int.Min(zeros, 8);
        }

        private static byte[][] ActualBitsCountTable =
        [
            [4, 6, 5, 7, 5, 7, 7, 5],
            [0, 0, 0, 0, 6, 8, 7, 5]
        ];

        /// <summary>
        /// There are 64 possible partition sets for a two-region tile.
        /// Each 4x4 block represents a single shape.
        /// Here also every fix-up index has MSB bit set.
        /// </summary>
        private static byte[][][][] PartitionSetsTable =
        [
            [
            [
                [128, 0, 1, 1],
                [0, 0, 1, 1],
                [0, 0, 1, 1],
                [0, 0, 1, 129]
            ],
            [
                [128, 0, 0, 1],
                [0, 0, 0, 1],
                [0, 0, 0, 1],
                [0, 0, 0, 129]
            ],
            [
                [128, 1, 1, 1],
                [0, 1, 1, 1],
                [0, 1, 1, 1],
                [0, 1, 1, 129]
            ],
            [
                [128, 0, 0, 1],
                [0, 0, 1, 1],
                [0, 0, 1, 1],
                [0, 1, 1, 129]
            ],
            [
                [128, 0, 0, 0],
                [0, 0, 0, 1],
                [0, 0, 0, 1],
                [0, 0, 1, 129]
            ],
            [
                [128, 0, 1, 1],
                [0, 1, 1, 1],
                [0, 1, 1, 1],
                [1, 1, 1, 129]
            ],
            [
                [128, 0, 0, 1],
                [0, 0, 1, 1],
                [0, 1, 1, 1],
                [1, 1, 1, 129]
            ],
            [
                [128, 0, 0, 0],
                [0, 0, 0, 1],
                [0, 0, 1, 1],
                [0, 1, 1, 129]
            ],
            [
                [128, 0, 0, 0],
                [0, 0, 0, 0],
                [0, 0, 0, 1],
                [0, 0, 1, 129]
            ],
            [
                [128, 0, 1, 1],
                [0, 1, 1, 1],
                [1, 1, 1, 1],
                [1, 1, 1, 129]
            ],
            [
                [128, 0, 0, 0],
                [0, 0, 0, 1],
                [0, 1, 1, 1],
                [1, 1, 1, 129]
            ],
            [
                [128, 0, 0, 0],
                [0, 0, 0, 0],
                [0, 0, 0, 1],
                [0, 1, 1, 129]
            ],
            [
                [128, 0, 0, 1],
                [0, 1, 1, 1],
                [1, 1, 1, 1],
                [1, 1, 1, 129]
            ],
            [
                [128, 0, 0, 0],
                [0, 0, 0, 0],
                [1, 1, 1, 1],
                [1, 1, 1, 129]
            ],
            [
                [128, 0, 0, 0],
                [1, 1, 1, 1],
                [1, 1, 1, 1],
                [1, 1, 1, 129]
            ],
            [
                [128, 0, 0, 0],
                [0, 0, 0, 0],
                [0, 0, 0, 0],
                [1, 1, 1, 129]
            ],
            [
                [128, 0, 0, 0],
                [1, 0, 0, 0],
                [1, 1, 1, 0],
                [1, 1, 1, 129]
            ],
            [
                [128, 1, 129, 1],
                [0, 0, 0, 1],
                [0, 0, 0, 0],
                [0, 0, 0, 0]
            ],
            [
                [128, 0, 0, 0],
                [0, 0, 0, 0],
                [129, 0, 0, 0],
                [1, 1, 1, 0]
            ],
            [
                [128, 1, 129, 1],
                [0, 0, 1, 1],
                [0, 0, 0, 1],
                [0, 0, 0, 0]
            ],
            [
                [128, 0, 129, 1],
                [0, 0, 0, 1],
                [0, 0, 0, 0],
                [0, 0, 0, 0]
            ],
            [
                [128, 0, 0, 0],
                [1, 0, 0, 0],
                [129, 1, 0, 0],
                [1, 1, 1, 0]
            ],
            [
                [128, 0, 0, 0],
                [0, 0, 0, 0],
                [129, 0, 0, 0],
                [1, 1, 0, 0]
            ],
            [
                [128, 1, 1, 1],
                [0, 0, 1, 1],
                [0, 0, 1, 1],
                [0, 0, 0, 129]
            ],
            [
                [128, 0, 129, 1],
                [0, 0, 0, 1],
                [0, 0, 0, 1],
                [0, 0, 0, 0]
            ],
            [
                [128, 0, 0, 0],
                [1, 0, 0, 0],
                [129, 0, 0, 0],
                [1, 1, 0, 0]
            ],
            [
                [128, 1, 129, 0],
                [0, 1, 1, 0],
                [0, 1, 1, 0],
                [0, 1, 1, 0]
            ],
            [
                [128, 0, 129, 1],
                [0, 1, 1, 0],
                [0, 1, 1, 0],
                [1, 1, 0, 0]
            ],
            [
                [128, 0, 0, 1],
                [0, 1, 1, 1],
                [129, 1, 1, 0],
                [1, 0, 0, 0]
            ],
            [
                [128, 0, 0, 0],
                [1, 1, 1, 1],
                [129, 1, 1, 1],
                [0, 0, 0, 0]
            ],
            [
                [128, 1, 129, 1],
                [0, 0, 0, 1],
                [1, 0, 0, 0],
                [1, 1, 1, 0]
            ],
            [
                [128, 0, 129, 1],
                [1, 0, 0, 1],
                [1, 0, 0, 1],
                [1, 1, 0, 0]
            ],
            [
                [128, 1, 0, 1],
                [0, 1, 0, 1],
                [0, 1, 0, 1],
                [0, 1, 0, 129]
            ],
            [
                [128, 0, 0, 0],
                [1, 1, 1, 1],
                [0, 0, 0, 0],
                [1, 1, 1, 129]
            ],
            [
                [128, 1, 0, 1],
                [1, 0, 129, 0],
                [0, 1, 0, 1],
                [1, 0, 1, 0]
            ],
            [
                [128, 0, 1, 1],
                [0, 0, 1, 1],
                [129, 1, 0, 0],
                [1, 1, 0, 0]
            ],
            [
                [128, 0, 129, 1],
                [1, 1, 0, 0],
                [0, 0, 1, 1],
                [1, 1, 0, 0]
            ],
            [
                [128, 1, 0, 1],
                [0, 1, 0, 1],
                [129, 0, 1, 0],
                [1, 0, 1, 0]
            ],
            [
                [128, 1, 1, 0],
                [1, 0, 0, 1],
                [0, 1, 1, 0],
                [1, 0, 0, 129]
            ],
            [
                [128, 1, 0, 1],
                [1, 0, 1, 0],
                [1, 0, 1, 0],
                [0, 1, 0, 129]
            ],
            [
                [128, 1, 129, 1],
                [0, 0, 1, 1],
                [1, 1, 0, 0],
                [1, 1, 1, 0]
            ],
            [
                [128, 0, 0, 1],
                [0, 0, 1, 1],
                [129, 1, 0, 0],
                [1, 0, 0, 0]
            ],
            [
                [128, 0, 129, 1],
                [0, 0, 1, 0],
                [0, 1, 0, 0],
                [1, 1, 0, 0]
            ],
            [
                [128, 0, 129, 1],
                [1, 0, 1, 1],
                [1, 1, 0, 1],
                [1, 1, 0, 0]
            ],
            [
                [128, 1, 129, 0],
                [1, 0, 0, 1],
                [1, 0, 0, 1],
                [0, 1, 1, 0]
            ],
            [
                [128, 0, 1, 1],
                [1, 1, 0, 0],
                [1, 1, 0, 0],
                [0, 0, 1, 129]
            ],
            [
                [128, 1, 1, 0],
                [0, 1, 1, 0],
                [1, 0, 0, 1],
                [1, 0, 0, 129]
            ],
            [
                [128, 0, 0, 0],
                [0, 1, 129, 0],
                [0, 1, 1, 0],
                [0, 0, 0, 0]
            ],
            [
                [128, 1, 0, 0],
                [1, 1, 129, 0],
                [0, 1, 0, 0],
                [0, 0, 0, 0]
            ],
            [
                [128, 0, 129, 0],
                [0, 1, 1, 1],
                [0, 0, 1, 0],
                [0, 0, 0, 0]
            ],
            [
                [128, 0, 0, 0],
                [0, 0, 129, 0],
                [0, 1, 1, 1],
                [0, 0, 1, 0]
            ],
            [
                [128, 0, 0, 0],
                [0, 1, 0, 0],
                [129, 1, 1, 0],
                [0, 1, 0, 0]
            ],
            [
                [128, 1, 1, 0],
                [1, 1, 0, 0],
                [1, 0, 0, 1],
                [0, 0, 1, 129]
            ],
            [
                [128, 0, 1, 1],
                [0, 1, 1, 0],
                [1, 1, 0, 0],
                [1, 0, 0, 129]
            ],
            [
                [128, 1, 129, 0],
                [0, 0, 1, 1],
                [1, 0, 0, 1],
                [1, 1, 0, 0]
            ],
            [
                [128, 0, 129, 1],
                [1, 0, 0, 1],
                [1, 1, 0, 0],
                [0, 1, 1, 0]
            ],
            [
                [128, 1, 1, 0],
                [1, 1, 0, 0],
                [1, 1, 0, 0],
                [1, 0, 0, 129]
            ],
            [
                [128, 1, 1, 0],
                [0, 0, 1, 1],
                [0, 0, 1, 1],
                [1, 0, 0, 129]
            ],
            [
                [128, 1, 1, 1],
                [1, 1, 1, 0],
                [1, 0, 0, 0],
                [0, 0, 0, 129]
            ],
            [
                [128, 0, 0, 1],
                [1, 0, 0, 0],
                [1, 1, 1, 0],
                [0, 1, 1, 129]
            ],
            [
                [128, 0, 0, 0],
                [1, 1, 1, 1],
                [0, 0, 1, 1],
                [0, 0, 1, 129]
            ],
            [
                [128, 0, 129, 1],
                [0, 0, 1, 1],
                [1, 1, 1, 1],
                [0, 0, 0, 0]
            ],
            [
                [128, 0, 129, 0],
                [0, 0, 1, 0],
                [1, 1, 1, 0],
                [1, 1, 1, 0]
            ],
            [
                [128, 1, 0, 0],
                [0, 1, 0, 0],
                [0, 1, 1, 1],
                [0, 1, 1, 129]
            ]
        ],
        [
            [
                [128, 0, 1, 129],
                [0, 0, 1, 1],
                [0, 2, 2, 1],
                [2, 2, 2, 130]
            ],
            [
                [128, 0, 0, 129],
                [0, 0, 1, 1],
                [130, 2, 1, 1],
                [2, 2, 2, 1]
            ],
            [
                [128, 0, 0, 0],
                [2, 0, 0, 1],
                [130, 2, 1, 1],
                [2, 2, 1, 129]
            ],
            [
                [128, 2, 2, 130],
                [0, 0, 2, 2],
                [0, 0, 1, 1],
                [0, 1, 1, 129]
            ],
            [
                [128, 0, 0, 0],
                [0, 0, 0, 0],
                [129, 1, 2, 2],
                [1, 1, 2, 130]
            ],
            [
                [128, 0, 1, 129],
                [0, 0, 1, 1],
                [0, 0, 2, 2],
                [0, 0, 2, 130]
            ],
            [
                [128, 0, 2, 130],
                [0, 0, 2, 2],
                [1, 1, 1, 1],
                [1, 1, 1, 129]
            ],
            [
                [128, 0, 1, 1],
                [0, 0, 1, 1],
                [130, 2, 1, 1],
                [2, 2, 1, 129]
            ],
            [
                [128, 0, 0, 0],
                [0, 0, 0, 0],
                [129, 1, 1, 1],
                [2, 2, 2, 130]
            ],
            [
                [128, 0, 0, 0],
                [1, 1, 1, 1],
                [129, 1, 1, 1],
                [2, 2, 2, 130]
            ],
            [
                [128, 0, 0, 0],
                [1, 1, 129, 1],
                [2, 2, 2, 2],
                [2, 2, 2, 130]
            ],
            [
                [128, 0, 1, 2],
                [0, 0, 129, 2],
                [0, 0, 1, 2],
                [0, 0, 1, 130]
            ],
            [
                [128, 1, 1, 2],
                [0, 1, 129, 2],
                [0, 1, 1, 2],
                [0, 1, 1, 130]
            ],
            [
                [128, 1, 2, 2],
                [0, 129, 2, 2],
                [0, 1, 2, 2],
                [0, 1, 2, 130]
            ],
            [
                [128, 0, 1, 129],
                [0, 1, 1, 2],
                [1, 1, 2, 2],
                [1, 2, 2, 130]
            ],
            [
                [128, 0, 1, 129],
                [2, 0, 0, 1],
                [130, 2, 0, 0],
                [2, 2, 2, 0]
            ],
            [
                [128, 0, 0, 129],
                [0, 0, 1, 1],
                [0, 1, 1, 2],
                [1, 1, 2, 130]
            ],
            [
                [128, 1, 1, 129],
                [0, 0, 1, 1],
                [130, 0, 0, 1],
                [2, 2, 0, 0]
            ],
            [
                [128, 0, 0, 0],
                [1, 1, 2, 2],
                [129, 1, 2, 2],
                [1, 1, 2, 130]
            ],
            [
                [128, 0, 2, 130],
                [0, 0, 2, 2],
                [0, 0, 2, 2],
                [1, 1, 1, 129]
            ],
            [
                [128, 1, 1, 129],
                [0, 1, 1, 1],
                [0, 2, 2, 2],
                [0, 2, 2, 130]
            ],
            [
                [128, 0, 0, 129],
                [0, 0, 0, 1],
                [130, 2, 2, 1],
                [2, 2, 2, 1]
            ],
            [
                [128, 0, 0, 0],
                [0, 0, 129, 1],
                [0, 1, 2, 2],
                [0, 1, 2, 130]
            ],
            [
                [128, 0, 0, 0],
                [1, 1, 0, 0],
                [130, 2, 129, 0],
                [2, 2, 1, 0]
            ],
            [
                [128, 1, 2, 130],
                [0, 129, 2, 2],
                [0, 0, 1, 1],
                [0, 0, 0, 0]
            ],
            [
                [128, 0, 1, 2],
                [0, 0, 1, 2],
                [129, 1, 2, 2],
                [2, 2, 2, 130]
            ],
            [
                [128, 1, 1, 0],
                [1, 2, 130, 1],
                [129, 2, 2, 1],
                [0, 1, 1, 0]
            ],
            [
                [128, 0, 0, 0],
                [0, 1, 129, 0],
                [1, 2, 130, 1],
                [1, 2, 2, 1]
            ],
            [
                [128, 0, 2, 2],
                [1, 1, 0, 2],
                [129, 1, 0, 2],
                [0, 0, 2, 130]
            ],
            [
                [128, 1, 1, 0],
                [0, 129, 1, 0],
                [2, 0, 0, 2],
                [2, 2, 2, 130]
            ],
            [
                [128, 0, 1, 1],
                [0, 1, 2, 2],
                [0, 1, 130, 2],
                [0, 0, 1, 129]
            ],
            [
                [128, 0, 0, 0],
                [2, 0, 0, 0],
                [130, 2, 1, 1],
                [2, 2, 2, 129]
            ],
            [
                [128, 0, 0, 0],
                [0, 0, 0, 2],
                [129, 1, 2, 2],
                [1, 2, 2, 130]
            ],
            [
                [128, 2, 2, 130],
                [0, 0, 2, 2],
                [0, 0, 1, 2],
                [0, 0, 1, 129]
            ],
            [
                [128, 0, 1, 129],
                [0, 0, 1, 2],
                [0, 0, 2, 2],
                [0, 2, 2, 130]
            ],
            [
                [128, 1, 2, 0],
                [0, 129, 2, 0],
                [0, 1, 130, 0],
                [0, 1, 2, 0]
            ],
            [
                [128, 0, 0, 0],
                [1, 1, 129, 1],
                [2, 2, 130, 2],
                [0, 0, 0, 0]
            ],
            [
                [128, 1, 2, 0],
                [1, 2, 0, 1],
                [130, 0, 129, 2],
                [0, 1, 2, 0]
            ],
            [
                [128, 1, 2, 0],
                [2, 0, 1, 2],
                [129, 130, 0, 1],
                [0, 1, 2, 0]
            ],
            [
                [128, 0, 1, 1],
                [2, 2, 0, 0],
                [1, 1, 130, 2],
                [0, 0, 1, 129]
            ],
            [
                [128, 0, 1, 1],
                [1, 1, 130, 2],
                [2, 2, 0, 0],
                [0, 0, 1, 129]
            ],
            [
                [128, 1, 0, 129],
                [0, 1, 0, 1],
                [2, 2, 2, 2],
                [2, 2, 2, 130]
            ],
            [
                [128, 0, 0, 0],
                [0, 0, 0, 0],
                [130, 1, 2, 1],
                [2, 1, 2, 129]
            ],
            [
                [128, 0, 2, 2],
                [1, 129, 2, 2],
                [0, 0, 2, 2],
                [1, 1, 2, 130]
            ],
            [
                [128, 0, 2, 130],
                [0, 0, 1, 1],
                [0, 0, 2, 2],
                [0, 0, 1, 129]
            ],
            [
                [128, 2, 2, 0],
                [1, 2, 130, 1],
                [0, 2, 2, 0],
                [1, 2, 2, 129]
            ],
            [
                [128, 1, 0, 1],
                [2, 2, 130, 2],
                [2, 2, 2, 2],
                [0, 1, 0, 129]
            ],
            [
                [128, 0, 0, 0],
                [2, 1, 2, 1],
                [130, 1, 2, 1],
                [2, 1, 2, 129]
            ],
            [
                [128, 1, 0, 129],
                [0, 1, 0, 1],
                [0, 1, 0, 1],
                [2, 2, 2, 130]
            ],
            [
                [128, 2, 2, 130],
                [0, 1, 1, 1],
                [0, 2, 2, 2],
                [0, 1, 1, 129]
            ],
            [
                [128, 0, 0, 2],
                [1, 129, 1, 2],
                [0, 0, 0, 2],
                [1, 1, 1, 130]
            ],
            [
                [128, 0, 0, 0],
                [2, 129, 1, 2],
                [2, 1, 1, 2],
                [2, 1, 1, 130]
            ],
            [
                [128, 2, 2, 2],
                [0, 129, 1, 1],
                [0, 1, 1, 1],
                [0, 2, 2, 130]
            ],
            [
                [128, 0, 0, 2],
                [1, 1, 1, 2],
                [129, 1, 1, 2],
                [0, 0, 0, 130]
            ],
            [
                [128, 1, 1, 0],
                [0, 129, 1, 0],
                [0, 1, 1, 0],
                [2, 2, 2, 130]
            ],
            [
                [128, 0, 0, 0],
                [0, 0, 0, 0],
                [2, 1, 129, 2],
                [2, 1, 1, 130]
            ],
            [
                [128, 1, 1, 0],
                [0, 129, 1, 0],
                [2, 2, 2, 2],
                [2, 2, 2, 130]
            ],
            [
                [128, 0, 2, 2],
                [0, 0, 1, 1],
                [0, 0, 129, 1],
                [0, 0, 2, 130]
            ],
            [
                [128, 0, 2, 2],
                [1, 1, 2, 2],
                [129, 1, 2, 2],
                [0, 0, 2, 130]
            ],
            [
                [128, 0, 0, 0],
                [0, 0, 0, 0],
                [0, 0, 0, 0],
                [2, 129, 1, 130]
            ],
            [
                [128, 0, 0, 130],
                [0, 0, 0, 1],
                [0, 0, 0, 2],
                [0, 0, 0, 129]
            ],
            [
                [128, 2, 2, 2],
                [1, 2, 2, 2],
                [0, 2, 2, 2],
                [129, 2, 2, 130]
            ],
            [
                [128, 1, 0, 129],
                [2, 2, 2, 2],
                [2, 2, 2, 2],
                [2, 2, 2, 130]
            ],
            [
                [128, 1, 1, 129],
                [2, 0, 1, 1],
                [130, 2, 0, 1],
                [2, 2, 2, 0]
            ]
        ]
        ];

        private static ReadOnlySpan<int> AWeight2Table => [0, 21, 43, 64];

        private static ReadOnlySpan<int> AWeight3Table => [0, 9, 18, 27, 37, 46, 55, 64];

        private static ReadOnlySpan<int> AWeight4Table => [0, 4, 9, 13, 17, 21, 26, 30, 34, 38, 43, 47, 51, 55, 60, 64];

        private const byte bcdec_bc7_sModeHasPBits = 0b11001011;
    }
}
