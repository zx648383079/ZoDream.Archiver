using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class PackedIntVectorConverter : BundleConverter<PackedIntVector>
    {
        public override PackedIntVector Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new PackedIntVector();
            res.NumItems = reader.ReadUInt32();
            int numData = reader.ReadInt32();
            if (numData > 0)
            {
                res.Data = reader.ReadBytes(numData);
            }
            reader.AlignStream();

            res.BitSize = reader.ReadByte();
            reader.AlignStream();
            return res;
        }

        public static int[] UnpackInts(PackedIntVector res)
        {
            var data = new int[res.NumItems];
            int indexPos = 0;
            int bitPos = 0;
            for (int i = 0; i < res.NumItems; i++)
            {
                int bits = 0;
                data[i] = 0;
                while (bits < res.BitSize)
                {
                    data[i] |= res.Data[indexPos] >> bitPos << bits;
                    int num = Math.Min(res.BitSize - bits, 8 - bitPos);
                    bitPos += num;
                    bits += num;
                    if (bitPos == 8)
                    {
                        indexPos++;
                        bitPos = 0;
                    }
                }
                data[i] &= (1 << res.BitSize) - 1;
            }
            return data;
        }
    }

}
