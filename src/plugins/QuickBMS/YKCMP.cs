using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using ZoDream.Shared;
using ZoDream.Shared.Bundle;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ZoDream.QuickBMS
{
    public class YKCMP
    {
        private static readonly string Magic = "YKCMP_V1";
        public static byte[] Decompress(Stream input)
        {
            using var reader = new BinaryReader(input);
            Expectation.ThrowIfNotSignature(reader.ReadBytes(8).Equal(Magic));
            var unknown = reader.ReadUInt32();
            var compressedSize = reader.ReadUInt32();
            var uncompressedSize = reader.ReadUInt32();
            var buffer = new byte[compressedSize];
            var output = new MemoryStream(buffer);
            while (reader.BaseStream.Position < compressedSize 
                && output.Position < uncompressedSize)
            {
                var flag = reader.ReadByte();

                if ((flag & 0x80) == 0x00)
                {
                    var data = reader.ReadBytes(flag);
                    output.Write(data, 0, data.Length);
                }
                else
                {
                    int size;
                    int offset;
                    //if (flag < 0xC0)
                    //{
                    //    size = (flag >> 4) - 0x08 + 0x01;
                    //    offset = (flag & 0x0F) + 0x01;
                    //}
                    //else if (flag < 0xE0)
                    //{
                    //    var tmp = reader.ReadByte();
                    //    size = flag - 0xC0 + 0x02;
                    //    offset = tmp + 0x01;
                    //}
                    //else
                    //{
                    //    var tmp = reader.ReadByte();
                    //    var tmp2 = reader.ReadByte();
                    //    size = (flag << 4) + (tmp >> 4) - 0xE00 + 0x03;
                    //    offset = ((tmp & 0x0F) << 8) + tmp2 + 0x01;
                    //}
                    if ((flag & 0x40) == 0x00)
                    {
                        size = (ushort)(((flag >> 4) & 0x03) + 1);
                        offset = size;
                    }
                    else /*if ((data & 0x20) == 0x00)*/
                    {
                        size = (ushort)((flag & 0x1F) + 2);
                        offset = reader.ReadByte();
                    }
                    var savepos = output.Position;
                    var data = new byte[size];
                    if (savepos < offset)
                    {
                        continue;
                        //var diff = offset - (int)savepos;
                        //if (diff < size)
                        //{
                        //    output.Seek(0, SeekOrigin.Begin);
                        //    output.Read(data, diff, size - diff);
                        //}
                        
                    } else
                    {
                        output.Seek(-offset, SeekOrigin.Current);
                        output.Read(data, 0, size);
                    }
                    
                    
                    output.Position = savepos;
                    output.Write(data, 0, data.Length);
                }
            }
            return buffer;
        }

        public static byte[] Compress(byte[] input)
        {
            var ms = new MemoryStream();
            var writer = new BinaryWriter(ms);
            writer.Write(Encoding.ASCII.GetBytes(Magic));
            writer.Write(4);
            //compressedSize skip
            writer.BaseStream.Position += 4;
            writer.Write(input.Length);

            //compression
            int pos = 0;
            var literals = new List<byte>();
            int compressedSize = 0x14;
            while (pos < input.Length)
            {
                var match = FindMatch(input, pos);
                if (match.Size == 0)
                {
                    if (literals.Count == 0x7F)
                    {
                        int writeCount = WriteLiterals(writer, literals);
                        compressedSize += writeCount;
                        literals.Clear();
                    }
                    literals.Add(input[pos]);
                    pos++;
                    continue;
                }
                if (literals.Count > 0)
                {
                    int writeCount = WriteLiterals(writer, literals);
                    compressedSize += writeCount;
                    literals.Clear();
                }

                if (match.Size <= 0x04 && match.Offset <= 0x10)
                {
                    int byte1 = (match.Size << 4) + 0x70 + (match.Offset - 1);
                    writer.Write((byte)byte1);
                    compressedSize += 1;
                }
                else if (match.Size <= 0x21 && match.Offset <= 0x100)
                {
                    int byte1 = match.Size + 0xC0 - 2;
                    int byte2 = match.Offset - 1;
                    writer.Write((byte)byte1);
                    writer.Write((byte)byte2);
                    compressedSize += 2;
                }
                else
                {
                    int tmp = match.Size + 0x0E00 - 3;
                    int byte1 = tmp >> 4;
                    int byte2 = ((tmp & 0x0F) << 4) + ((match.Offset - 1) >> 8);
                    int byte3 = match.Offset - 1;
                    writer.Write((byte)byte1);
                    writer.Write((byte)byte2);
                    writer.Write((byte)byte3);
                    compressedSize += 3;
                }
                pos += match.Size;
            }

            if (literals.Count > 0)
            {
                int writeCount = WriteLiterals(writer, literals);
                compressedSize += writeCount;
                literals.Clear();
            }

            writer.BaseStream.Position = 0x0C;
            writer.Write(compressedSize);
            return ms.ToArray();
        }

        private readonly struct MatchInfo(int offset, int size)
        {
            public readonly int Size = size;
            public readonly int Offset = offset;
        }
        private static int WriteLiterals(BinaryWriter writer, List<byte> literals)
        {
            writer.Write((byte)literals.Count);
            writer.Write(literals.ToArray());
            return literals.Count + 1;
        }
        private static MatchInfo FindMatch(byte[] data, int pos)
        {
            int[] maxSize = [0x03, 0x1F, 0x1FF];
            int[] maxOffset = [0x10, 0x100, 0x202];

            int size = 0;
            int offset = 0;
            int saved = 0;

            for (int i = 3; i > 0; i--)
            {
                int start = Math.Max(pos - maxOffset[i - 1], 0);
                int end = pos - saved - i;
                int current = start;
                while (current < end)
                {
                    if (data[current] != data[pos])
                    {
                        current++;
                        continue;
                    }

                    int matchSize = MatchLength(data, current, pos, maxSize[i - 1] + i);

                    if ((matchSize - i) > saved)
                    {
                        size = matchSize;
                        offset = pos - current;
                        saved = matchSize - i;
                        end = pos - saved - i;
                    }

                    current++;
                }
            }

            if (saved > 0)
            {
                return new MatchInfo(offset, size);
            }

            return new MatchInfo(-1, 0);
        }

        private static int MatchLength(byte[] data, int start, int end, int maxSize)
        {
            int currentLength = 0;
            int pos1 = start;
            int pos2 = end;
            while (pos1 < end && pos2 < data.Length && data[pos1] == data[pos2] && currentLength < maxSize)
            {
                pos1++;
                pos2++;
                currentLength++;
            }

            return currentLength;
        }
    }
}
