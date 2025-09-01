using System;
using System.Buffers.Binary;

namespace ZoDream.Shared.Drawing
{
    public class BC6h(bool isSigned) : BlockBufferDecoder
    {

        protected override int BlockSize => 16;
        /// <summary>
        /// RGB half
        /// </summary>
        protected override int BlockPixelSize => 6;

        protected override void DecodeBlock(ReadOnlySpan<byte> data, Span<byte> output)
        {
            var bstream = new BitStream(data);
            Span<uint> r = stackalloc uint[4]; // wxyz
            Span<uint> g = stackalloc uint[4];
            Span<uint> b = stackalloc uint[4];

            int decompressedOffset = 0;

            r.Clear();
            g.Clear();
            b.Clear();

            int mode;

            //modes >= 11 (10 in my code) are using 0 one, others will read it from the bitstream
            uint partition;

            switch (ReadBc6hModeBits(ref bstream))
            {
                // mode 1 
                case 0b00:
                    {
                        // Partitition indices: 46 bits
                        // Partition: 5 bits
                        // Color Endpoints: 75 bits (10.555, 10.555, 10.555)
                        g[2] |= bstream.ReadBit() << 4; // gy[4]
                        b[2] |= bstream.ReadBit() << 4; // by[4]
                        b[3] |= bstream.ReadBit() << 4; // bz[4]
                        r[0] |= bstream.ReadBits(10); // rw[9:0]
                        g[0] |= bstream.ReadBits(10); // gw[9:0]
                        b[0] |= bstream.ReadBits(10); // bw[9:0]
                        r[1] |= bstream.ReadBits(5); // rx[4:0]
                        g[3] |= bstream.ReadBit() << 4; // gz[4]
                        g[2] |= bstream.ReadBits(4); // gy[3:0]
                        g[1] |= bstream.ReadBits(5); // gx[4:0]
                        b[3] |= bstream.ReadBit(); // bz[0]
                        g[3] |= bstream.ReadBits(4); // gz[3:0]
                        b[1] |= bstream.ReadBits(5); // bx[4:0]
                        b[3] |= bstream.ReadBit() << 1; // bz[1]
                        b[2] |= bstream.ReadBits(4); // by[3:0]
                        r[2] |= bstream.ReadBits(5); // ry[4:0]
                        b[3] |= bstream.ReadBit() << 2; // bz[2]
                        r[3] |= bstream.ReadBits(5); // rz[4:0]
                        b[3] |= bstream.ReadBit() << 3; // bz[3]
                        partition = bstream.ReadBits(5); // d[4:0]
                        mode = 0;
                    }
                    break;

                // mode 2 
                case 0b01:
                    {
                        // Partitition indices: 46 bits
                        // Partition: 5 bits
                        // Color Endpoints: 75 bits (7666, 7666, 7666)
                        g[2] |= bstream.ReadBit() << 5; // gy[5]
                        g[3] |= bstream.ReadBit() << 4; // gz[4]
                        g[3] |= bstream.ReadBit() << 5; // gz[5]
                        r[0] |= bstream.ReadBits(7); // rw[6:0]
                        b[3] |= bstream.ReadBit(); // bz[0]
                        b[3] |= bstream.ReadBit() << 1; // bz[1]
                        b[2] |= bstream.ReadBit() << 4; // by[4]
                        g[0] |= bstream.ReadBits(7); // gw[6:0]
                        b[2] |= bstream.ReadBit() << 5; // by[5]
                        b[3] |= bstream.ReadBit() << 2; // bz[2]
                        g[2] |= bstream.ReadBit() << 4; // gy[4]
                        b[0] |= bstream.ReadBits(7); // bw[6:0]
                        b[3] |= bstream.ReadBit() << 3; // bz[3]
                        b[3] |= bstream.ReadBit() << 5; // bz[5]
                        b[3] |= bstream.ReadBit() << 4; // bz[4]
                        r[1] |= bstream.ReadBits(6); // rx[5:0]
                        g[2] |= bstream.ReadBits(4); // gy[3:0]
                        g[1] |= bstream.ReadBits(6); // gx[5:0]
                        g[3] |= bstream.ReadBits(4); // gz[3:0]
                        b[1] |= bstream.ReadBits(6); // bx[5:0]
                        b[2] |= bstream.ReadBits(4); // by[3:0]
                        r[2] |= bstream.ReadBits(6); // ry[5:0]
                        r[3] |= bstream.ReadBits(6); // rz[5:0]
                        partition = bstream.ReadBits(5); // d[4:0]
                        mode = 1;
                    }
                    break;

                // mode 3 
                case 0b00010:
                    {
                        // Partitition indices: 46 bits
                        // Partition: 5 bits
                        // Color Endpoints: 72 bits (11.555, 11.444, 11.444)
                        r[0] |= bstream.ReadBits(10); // rw[9:0]
                        g[0] |= bstream.ReadBits(10); // gw[9:0]
                        b[0] |= bstream.ReadBits(10); // bw[9:0]
                        r[1] |= bstream.ReadBits(5); // rx[4:0]
                        r[0] |= bstream.ReadBit() << 10; // rw[10]
                        g[2] |= bstream.ReadBits(4); // gy[3:0]
                        g[1] |= bstream.ReadBits(4); // gx[3:0]
                        g[0] |= bstream.ReadBit() << 10; // gw[10]
                        b[3] |= bstream.ReadBit(); // bz[0]
                        g[3] |= bstream.ReadBits(4); // gz[3:0]
                        b[1] |= bstream.ReadBits(4); // bx[3:0]
                        b[0] |= bstream.ReadBit() << 10; // bw[10]
                        b[3] |= bstream.ReadBit() << 1; // bz[1]
                        b[2] |= bstream.ReadBits(4); // by[3:0]
                        r[2] |= bstream.ReadBits(5); // ry[4:0]
                        b[3] |= bstream.ReadBit() << 2; // bz[2]
                        r[3] |= bstream.ReadBits(5); // rz[4:0]
                        b[3] |= bstream.ReadBit() << 3; // bz[3]
                        partition = bstream.ReadBits(5); // d[4:0]
                        mode = 2;
                    }
                    break;

                // mode 4 
                case 0b00110:
                    {
                        // Partitition indices: 46 bits
                        // Partition: 5 bits
                        // Color Endpoints: 72 bits (11.444, 11.555, 11.444)
                        r[0] |= bstream.ReadBits(10); // rw[9:0]
                        g[0] |= bstream.ReadBits(10); // gw[9:0]
                        b[0] |= bstream.ReadBits(10); // bw[9:0]
                        r[1] |= bstream.ReadBits(4); // rx[3:0]
                        r[0] |= bstream.ReadBit() << 10; // rw[10]
                        g[3] |= bstream.ReadBit() << 4; // gz[4]
                        g[2] |= bstream.ReadBits(4); // gy[3:0]
                        g[1] |= bstream.ReadBits(5); // gx[4:0]
                        g[0] |= bstream.ReadBit() << 10; // gw[10]
                        g[3] |= bstream.ReadBits(4); // gz[3:0]
                        b[1] |= bstream.ReadBits(4); // bx[3:0]
                        b[0] |= bstream.ReadBit() << 10; // bw[10]
                        b[3] |= bstream.ReadBit() << 1; // bz[1]
                        b[2] |= bstream.ReadBits(4); // by[3:0]
                        r[2] |= bstream.ReadBits(4); // ry[3:0]
                        b[3] |= bstream.ReadBit(); // bz[0]
                        b[3] |= bstream.ReadBit() << 2; // bz[2]
                        r[3] |= bstream.ReadBits(4); // rz[3:0]
                        g[2] |= bstream.ReadBit() << 4; // gy[4]
                        b[3] |= bstream.ReadBit() << 3; // bz[3]
                        partition = bstream.ReadBits(5); // d[4:0]
                        mode = 3;
                    }
                    break;

                // mode 5 
                case 0b01010:
                    {
                        // Partitition indices: 46 bits
                        // Partition: 5 bits
                        // Color Endpoints: 72 bits (11.444, 11.444, 11.555)
                        r[0] |= bstream.ReadBits(10); // rw[9:0]
                        g[0] |= bstream.ReadBits(10); // gw[9:0]
                        b[0] |= bstream.ReadBits(10); // bw[9:0]
                        r[1] |= bstream.ReadBits(4); // rx[3:0]
                        r[0] |= bstream.ReadBit() << 10; // rw[10]
                        b[2] |= bstream.ReadBit() << 4; // by[4]
                        g[2] |= bstream.ReadBits(4); // gy[3:0]
                        g[1] |= bstream.ReadBits(4); // gx[3:0]
                        g[0] |= bstream.ReadBit() << 10; // gw[10]
                        b[3] |= bstream.ReadBit(); // bz[0]
                        g[3] |= bstream.ReadBits(4); // gz[3:0]
                        b[1] |= bstream.ReadBits(5); // bx[4:0]
                        b[0] |= bstream.ReadBit() << 10; // bw[10]
                        b[2] |= bstream.ReadBits(4); // by[3:0]
                        r[2] |= bstream.ReadBits(4); // ry[3:0]
                        b[3] |= bstream.ReadBit() << 1; // bz[1]
                        b[3] |= bstream.ReadBit() << 2; // bz[2]
                        r[3] |= bstream.ReadBits(4); // rz[3:0]
                        b[3] |= bstream.ReadBit() << 4; // bz[4]
                        b[3] |= bstream.ReadBit() << 3; // bz[3]
                        partition = bstream.ReadBits(5); // d[4:0]
                        mode = 4;
                    }
                    break;

                // mode 6 
                case 0b01110:
                    {
                        // Partitition indices: 46 bits
                        // Partition: 5 bits
                        // Color Endpoints: 72 bits (9555, 9555, 9555)
                        r[0] |= bstream.ReadBits(9); // rw[8:0]
                        b[2] |= bstream.ReadBit() << 4; // by[4]
                        g[0] |= bstream.ReadBits(9); // gw[8:0]
                        g[2] |= bstream.ReadBit() << 4; // gy[4]
                        b[0] |= bstream.ReadBits(9); // bw[8:0]
                        b[3] |= bstream.ReadBit() << 4; // bz[4]
                        r[1] |= bstream.ReadBits(5); // rx[4:0]
                        g[3] |= bstream.ReadBit() << 4; // gz[4]
                        g[2] |= bstream.ReadBits(4); // gy[3:0]
                        g[1] |= bstream.ReadBits(5); // gx[4:0]
                        b[3] |= bstream.ReadBit(); // bz[0]
                        g[3] |= bstream.ReadBits(4); // gx[3:0]
                        b[1] |= bstream.ReadBits(5); // bx[4:0]
                        b[3] |= bstream.ReadBit() << 1; // bz[1]
                        b[2] |= bstream.ReadBits(4); // by[3:0]
                        r[2] |= bstream.ReadBits(5); // ry[4:0]
                        b[3] |= bstream.ReadBit() << 2; // bz[2]
                        r[3] |= bstream.ReadBits(5); // rz[4:0]
                        b[3] |= bstream.ReadBit() << 3; // bz[3]
                        partition = bstream.ReadBits(5); // d[4:0]
                        mode = 5;
                    }
                    break;

                // mode 7 
                case 0b10010:
                    {
                        // Partitition indices: 46 bits
                        // Partition: 5 bits
                        // Color Endpoints: 72 bits (8666, 8555, 8555)
                        r[0] |= bstream.ReadBits(8); // rw[7:0]
                        g[3] |= bstream.ReadBit() << 4; // gz[4]
                        b[2] |= bstream.ReadBit() << 4; // by[4]
                        g[0] |= bstream.ReadBits(8); // gw[7:0]
                        b[3] |= bstream.ReadBit() << 2; // bz[2]
                        g[2] |= bstream.ReadBit() << 4; // gy[4]
                        b[0] |= bstream.ReadBits(8); // bw[7:0]
                        b[3] |= bstream.ReadBit() << 3; // bz[3]
                        b[3] |= bstream.ReadBit() << 4; // bz[4]
                        r[1] |= bstream.ReadBits(6); // rx[5:0]
                        g[2] |= bstream.ReadBits(4); // gy[3:0]
                        g[1] |= bstream.ReadBits(5); // gx[4:0]
                        b[3] |= bstream.ReadBit(); // bz[0]
                        g[3] |= bstream.ReadBits(4); // gz[3:0]
                        b[1] |= bstream.ReadBits(5); // bx[4:0]
                        b[3] |= bstream.ReadBit() << 1; // bz[1]
                        b[2] |= bstream.ReadBits(4); // by[3:0]
                        r[2] |= bstream.ReadBits(6); // ry[5:0]
                        r[3] |= bstream.ReadBits(6); // rz[5:0]
                        partition = bstream.ReadBits(5); // d[4:0]
                        mode = 6;
                    }
                    break;

                // mode 8 
                case 0b10110:
                    {
                        // Partitition indices: 46 bits
                        // Partition: 5 bits
                        // Color Endpoints: 72 bits (8555, 8666, 8555)
                        r[0] |= bstream.ReadBits(8); // rw[7:0]
                        b[3] |= bstream.ReadBit(); // bz[0]
                        b[2] |= bstream.ReadBit() << 4; // by[4]
                        g[0] |= bstream.ReadBits(8); // gw[7:0]
                        g[2] |= bstream.ReadBit() << 5; // gy[5]
                        g[2] |= bstream.ReadBit() << 4; // gy[4]
                        b[0] |= bstream.ReadBits(8); // bw[7:0]
                        g[3] |= bstream.ReadBit() << 5; // gz[5]
                        b[3] |= bstream.ReadBit() << 4; // bz[4]
                        r[1] |= bstream.ReadBits(5); // rx[4:0]
                        g[3] |= bstream.ReadBit() << 4; // gz[4]
                        g[2] |= bstream.ReadBits(4); // gy[3:0]
                        g[1] |= bstream.ReadBits(6); // gx[5:0]
                        g[3] |= bstream.ReadBits(4); // zx[3:0]
                        b[1] |= bstream.ReadBits(5); // bx[4:0]
                        b[3] |= bstream.ReadBit() << 1; // bz[1]
                        b[2] |= bstream.ReadBits(4); // by[3:0]
                        r[2] |= bstream.ReadBits(5); // ry[4:0]
                        b[3] |= bstream.ReadBit() << 2; // bz[2]
                        r[3] |= bstream.ReadBits(5); // rz[4:0]
                        b[3] |= bstream.ReadBit() << 3; // bz[3]
                        partition = bstream.ReadBits(5); // d[4:0]
                        mode = 7;
                    }
                    break;

                // mode 9 
                case 0b11010:
                    {
                        // Partitition indices: 46 bits
                        // Partition: 5 bits
                        // Color Endpoints: 72 bits (8555, 8555, 8666)
                        r[0] |= bstream.ReadBits(8); // rw[7:0]
                        b[3] |= bstream.ReadBit() << 1; // bz[1]
                        b[2] |= bstream.ReadBit() << 4; // by[4]
                        g[0] |= bstream.ReadBits(8); // gw[7:0]
                        b[2] |= bstream.ReadBit() << 5; // by[5]
                        g[2] |= bstream.ReadBit() << 4; // gy[4]
                        b[0] |= bstream.ReadBits(8); // bw[7:0]
                        b[3] |= bstream.ReadBit() << 5; // bz[5]
                        b[3] |= bstream.ReadBit() << 4; // bz[4]
                        r[1] |= bstream.ReadBits(5); // bw[4:0]
                        g[3] |= bstream.ReadBit() << 4; // gz[4]
                        g[2] |= bstream.ReadBits(4); // gy[3:0]
                        g[1] |= bstream.ReadBits(5); // gx[4:0]
                        b[3] |= bstream.ReadBit(); // bz[0]
                        g[3] |= bstream.ReadBits(4); // gz[3:0]
                        b[1] |= bstream.ReadBits(6); // bx[5:0]
                        b[2] |= bstream.ReadBits(4); // by[3:0]
                        r[2] |= bstream.ReadBits(5); // ry[4:0]
                        b[3] |= bstream.ReadBit() << 2; // bz[2]
                        r[3] |= bstream.ReadBits(5); // rz[4:0]
                        b[3] |= bstream.ReadBit() << 3; // bz[3]
                        partition = bstream.ReadBits(5); // d[4:0]
                        mode = 8;
                    }
                    break;

                // mode 10 
                case 0b11110:
                    {
                        // Partitition indices: 46 bits
                        // Partition: 5 bits
                        // Color Endpoints: 72 bits (6666, 6666, 6666)
                        r[0] |= bstream.ReadBits(6); // rw[5:0]
                        g[3] |= bstream.ReadBit() << 4; // gz[4]
                        b[3] |= bstream.ReadBit(); // bz[0]
                        b[3] |= bstream.ReadBit() << 1; // bz[1]
                        b[2] |= bstream.ReadBit() << 4; // by[4]
                        g[0] |= bstream.ReadBits(6); // gw[5:0]
                        g[2] |= bstream.ReadBit() << 5; // gy[5]
                        b[2] |= bstream.ReadBit() << 5; // by[5]
                        b[3] |= bstream.ReadBit() << 2; // bz[2]
                        g[2] |= bstream.ReadBit() << 4; // gy[4]
                        b[0] |= bstream.ReadBits(6); // bw[5:0]
                        g[3] |= bstream.ReadBit() << 5; // gz[5]
                        b[3] |= bstream.ReadBit() << 3; // bz[3]
                        b[3] |= bstream.ReadBit() << 5; // bz[5]
                        b[3] |= bstream.ReadBit() << 4; // bz[4]
                        r[1] |= bstream.ReadBits(6); // rx[5:0]
                        g[2] |= bstream.ReadBits(4); // gy[3:0]
                        g[1] |= bstream.ReadBits(6); // gx[5:0]
                        g[3] |= bstream.ReadBits(4); // gz[3:0]
                        b[1] |= bstream.ReadBits(6); // bx[5:0]
                        b[2] |= bstream.ReadBits(4); // by[3:0]
                        r[2] |= bstream.ReadBits(6); // ry[5:0]
                        r[3] |= bstream.ReadBits(6); // rz[5:0]
                        partition = bstream.ReadBits(5); // d[4:0]
                        mode = 9;
                    }
                    break;

                // mode 11 
                case 0b00011:
                    {
                        // Partitition indices: 63 bits
                        // Partition: 0 bits
                        // Color Endpoints: 60 bits (10.10, 10.10, 10.10)
                        r[0] |= bstream.ReadBits(10); // rw[9:0]
                        g[0] |= bstream.ReadBits(10); // gw[9:0]
                        b[0] |= bstream.ReadBits(10); // bw[9:0]
                        r[1] |= bstream.ReadBits(10); // rx[9:0]
                        g[1] |= bstream.ReadBits(10); // gx[9:0]
                        b[1] |= bstream.ReadBits(10); // bx[9:0]
                        partition = 0;
                        mode = 10;
                    }
                    break;

                // mode 12 
                case 0b00111:
                    {
                        // Partitition indices: 63 bits
                        // Partition: 0 bits
                        // Color Endpoints: 60 bits (11.9, 11.9, 11.9)
                        r[0] |= bstream.ReadBits(10); // rw[9:0]
                        g[0] |= bstream.ReadBits(10); // gw[9:0]
                        b[0] |= bstream.ReadBits(10); // bw[9:0]
                        r[1] |= bstream.ReadBits(9); // rx[8:0]
                        r[0] |= bstream.ReadBit() << 10; // rw[10]
                        g[1] |= bstream.ReadBits(9); // gx[8:0]
                        g[0] |= bstream.ReadBit() << 10; // gw[10]
                        b[1] |= bstream.ReadBits(9); // bx[8:0]
                        b[0] |= bstream.ReadBit() << 10; // bw[10]
                        partition = 0;
                        mode = 11;
                    }
                    break;

                // mode 13 
                case 0b01011:
                    {
                        // Partitition indices: 63 bits
                        // Partition: 0 bits
                        // Color Endpoints: 60 bits (12.8, 12.8, 12.8)
                        r[0] |= bstream.ReadBits(10); // rw[9:0]
                        g[0] |= bstream.ReadBits(10); // gw[9:0]
                        b[0] |= bstream.ReadBits(10); // bw[9:0]
                        r[1] |= bstream.ReadBits(8); // rx[7:0]
                        r[0] |= bstream.ReadBitsReversed(2) << 10; // rx[10:11]
                        g[1] |= bstream.ReadBits(8); // gx[7:0]
                        g[0] |= bstream.ReadBitsReversed(2) << 10; // gx[10:11]
                        b[1] |= bstream.ReadBits(8); // bx[7:0]
                        b[0] |= bstream.ReadBitsReversed(2) << 10; // bx[10:11]
                        partition = 0;
                        mode = 12;
                    }
                    break;

                // mode 14 
                case 0b01111:
                    {
                        // Partitition indices: 63 bits
                        // Partition: 0 bits
                        // Color Endpoints: 60 bits (16.4, 16.4, 16.4)
                        r[0] |= bstream.ReadBits(10); // rw[9:0]
                        g[0] |= bstream.ReadBits(10); // gw[9:0]
                        b[0] |= bstream.ReadBits(10); // bw[9:0]
                        r[1] |= bstream.ReadBits(4); // rx[3:0]
                        r[0] |= bstream.ReadBitsReversed(6) << 10; // rw[10:15]
                        g[1] |= bstream.ReadBits(4); // gx[3:0]
                        g[0] |= bstream.ReadBitsReversed(6) << 10; // gw[10:15]
                        b[1] |= bstream.ReadBits(4); // bx[3:0]
                        b[0] |= bstream.ReadBitsReversed(6) << 10; // bw[10:15]
                        partition = 0;
                        mode = 13;
                    }
                    break;

                default:
                    {
                        // Modes 10011, 10111, 11011, and 11111(not shown) are reserved.
                        // Do not use these in your encoder. If the hardware is passed blocks
                        // with one of these modes specified, the resulting decompressed block
                        // must contain all zeroes in all channels except for the alpha channel.
                        output.Clear();

                        return;
                    }
            }

            int numPartitions = mode >= 10 ? 0 : 1;
            byte actualBits0Mode = ActualBitsCountTable[0][mode];
            if (isSigned)
            {
                r[0] = ExtendSign(r[0], actualBits0Mode);
                g[0] = ExtendSign(g[0], actualBits0Mode);
                b[0] = ExtendSign(b[0], actualBits0Mode);
            }

            // Mode 11 (like Mode 10) does not use delta compression,
            // and instead stores both color endpoints explicitly.
            if ((mode is not 9 and not 10) || isSigned)
            {
                for (int i = 1; i < (numPartitions + 1) * 2; ++i)
                {
                    r[i] = ExtendSign(r[i], ActualBitsCountTable[1][mode]);
                    g[i] = ExtendSign(g[i], ActualBitsCountTable[2][mode]);
                    b[i] = ExtendSign(b[i], ActualBitsCountTable[3][mode]);
                }
            }

            if (mode is not 9 and not 10)
            {
                for (int i = 1; i < (numPartitions + 1) * 2; ++i)
                {
                    r[i] = TransformInverse(r[i], r[0], actualBits0Mode, isSigned);
                    g[i] = TransformInverse(g[i], g[0], actualBits0Mode, isSigned);
                    b[i] = TransformInverse(b[i], b[0], actualBits0Mode, isSigned);
                }
            }

            for (int i = 0; i < (numPartitions + 1) * 2; i++)
            {
                r[i] = Unquantize(r[i], actualBits0Mode, isSigned);
                g[i] = Unquantize(g[i], actualBits0Mode, isSigned);
                b[i] = Unquantize(b[i], actualBits0Mode, isSigned);
            }

            ReadOnlySpan<int> weights = (mode >= 10) ? AWeight4Table : AWeight3Table;
            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    int partitionSet = (mode >= 10) ? ((i | j) != 0 ? 0 : 128) : PartitionSetsTable[partition][i][j];

                    int indexBits = (mode >= 10) ? 4 : 3;
                    // fix-up index is specified with one less bit 
                    // The fix-up index for subset 0 is always index 0 
                    if ((partitionSet & 0x80) != 0)
                    {
                        indexBits--;
                    }
                    partitionSet &= 0x01;

                    int index = (int)bstream.ReadBits(indexBits);

                    int ep_i = (partitionSet * 2);

                    int pixelOffset = decompressedOffset + (j * 3 * sizeof(ushort));

                    int rOffset = pixelOffset + 0 * sizeof(ushort);
                    ushort rFinal = FinishUnquantize(Interpolate(r[ep_i], r[ep_i + 1], weights, index), isSigned);
                    BinaryPrimitives.WriteUInt16LittleEndian(output.Slice(rOffset), rFinal);

                    int gOffset = pixelOffset + 1 * sizeof(ushort);
                    ushort gFinal = FinishUnquantize(Interpolate(g[ep_i], g[ep_i + 1], weights, index), isSigned);
                    BinaryPrimitives.WriteUInt16LittleEndian(output.Slice(gOffset), gFinal);

                    int bOffset = pixelOffset + 2 * sizeof(ushort);
                    ushort bFinal = FinishUnquantize(Interpolate(b[ep_i], b[ep_i + 1], weights, index), isSigned);
                    BinaryPrimitives.WriteUInt16LittleEndian(output.Slice(bOffset), bFinal);
                }

                decompressedOffset += 4 * 3 * sizeof(ushort);
            }

            
        }

        protected override void CopyPixelTo(ReadOnlySpan<byte> data, int index, Span<byte> output)
        {
            var offset = index * BlockPixelSize;
            output[0] = ColorConverter.FromHalfToByte(data[offset..]);
            output[1] = ColorConverter.FromHalfToByte(data[(offset + 2)..]);
            output[2] = ColorConverter.FromHalfToByte(data[(offset + 4)..]);
            output[3] = byte.MaxValue;
        }

        private static uint ReadBc6hModeBits(ref BitStream bstream)
        {
            uint twoBits = bstream.ReadBits(2);
            if (twoBits > 1)
            {
                uint threeBits = bstream.ReadBits(3);
                return (threeBits << 2) | twoBits;
            }
            else
            {
                return twoBits;
            }
        }

        private static int ExtendSign(int val, int bits)
        {
            return (val << (32 - bits)) >> (32 - bits);
        }

        private static uint ExtendSign(uint val, int bits)
        {
            return (uint)ExtendSign((int)val, bits);
        }

        public static int TransformInverse(int val, int a0, int bits, bool isSigned)
        {
            // If the precision of A0 is "p" bits, then the transform algorithm is:
            // B0 = (B0 + A0) & ((1 << p) - 1)
            val = (val + a0) & ((1 << bits) - 1);
            if (isSigned)
            {
                val = ExtendSign(val, bits);
            }
            return val;
        }

        private static uint TransformInverse(uint val, uint a0, int bits, bool isSigned)
        {
            return (uint)TransformInverse((int)val, (int)a0, bits, isSigned);
        }

        /// <summary>
        /// Essentially copy-paste from documentation
        /// </summary>
        /// <param name="val"></param>
        /// <param name="bits"></param>
        /// <param name="isSigned"></param>
        /// <returns></returns>
        public static int Unquantize(int val, int bits, bool isSigned)
        {
            int unq;
            int s = 0;

            if (!isSigned)
            {
                if (bits >= 15)
                {
                    unq = val;
                }
                else if (val == 0)
                {
                    unq = 0;
                }
                else if (val == ((1 << bits) - 1))
                {
                    unq = 0xFFFF;
                }
                else
                {
                    unq = ((val << 16) + 0x8000) >> bits;
                }
            }
            else
            {
                if (bits >= 16)
                {
                    unq = val;
                }
                else
                {
                    if (val < 0)
                    {
                        s = 1;
                        val = -val;
                    }

                    if (val == 0)
                    {
                        unq = 0;
                    }
                    else if (val >= ((1 << (bits - 1)) - 1))
                    {
                        unq = 0x7FFF;
                    }
                    else
                    {
                        unq = ((val << 15) + 0x4000) >> (bits - 1);
                    }

                    if (s != 0)
                    {
                        unq = -unq;
                    }
                }
            }
            return unq;
        }

        private static uint Unquantize(uint val, int bits, bool isSigned)
        {
            return (uint)Unquantize((int)val, bits, isSigned);
        }

        internal static int Interpolate(int a, int b, ReadOnlySpan<int> weights, int index)
        {
            return ((a * (64 - weights[index])) + (b * weights[index]) + 32) >> 6;
        }

        internal static uint Interpolate(uint a, uint b, ReadOnlySpan<int> weights, int index)
        {
            return (uint)Interpolate((int)a, (int)b, weights, index);
        }

        public static ushort FinishUnquantize(int val, bool isSigned)
        {
            if (!isSigned)
            {
                return (ushort)((val * 31) >> 6); // scale the magnitude by 31 / 64
            }
            else
            {
                val = (val < 0) ? (((-val) * 31) >> 5) : (val * 31) >> 5; // scale the magnitude by 31 / 32
                int s = 0;
                if (val < 0)
                {
                    s = 0x8000;
                    val = -val;
                }
                return (ushort)(s | val);
            }
        }

        private static ushort FinishUnquantize(uint val, bool isSigned)
        {
            return FinishUnquantize((int)val, isSigned);
        }


        private static byte[][] ActualBitsCountTable =
        [
            [10, 7, 11, 11, 11, 9, 8, 8, 8, 6, 10, 11, 12, 16],
                [5, 6, 5, 4, 4, 5, 6, 5, 5, 6, 10, 9, 8, 4],
                [5, 6, 4, 5, 4, 5, 5, 6, 5, 6, 10, 9, 8, 4],
                [5, 6, 4, 4, 5, 5, 5, 5, 6, 6, 10, 9, 8, 4]
        ];

        /// <summary>
        /// There are 32 possible partition sets for a two-region tile.
        /// Each 4x4 block represents a single shape.
        /// Here also every fix-up index has MSB bit set.
        /// </summary>
        private static byte[][][] PartitionSetsTable =
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
        ]
        ];

        private static ReadOnlySpan<int> AWeight3Table => [0, 9, 18, 27, 37, 46, 55, 64];

        private static ReadOnlySpan<int> AWeight4Table => [0, 4, 9, 13, 17, 21, 26, 30, 34, 38, 43, 47, 51, 55, 60, 64];
    }
}
