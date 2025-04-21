using System;
using System.Numerics;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class PackedQuatVectorConverter : BundleConverter<PackedQuatVector>
    {
        public override PackedQuatVector Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new PackedQuatVector();
            res.NumItems = reader.ReadUInt32();

            int numData = reader.ReadInt32();
            res.Data = reader.ReadBytes(numData);

            reader.AlignStream();
            return res;
        }

        public Quaternion[] UnpackQuats(PackedQuatVector res)
        {
            var data = new Quaternion[res.NumItems];
            int indexPos = 0;
            int bitPos = 0;

            for (int i = 0; i < res.NumItems; i++)
            {
                uint flags = 0;

                int bits = 0;
                while (bits < 3)
                {
                    flags |= (uint)(res.Data[indexPos] >> bitPos << bits);
                    int num = Math.Min(3 - bits, 8 - bitPos);
                    bitPos += num;
                    bits += num;
                    if (bitPos == 8)
                    {
                        indexPos++;
                        bitPos = 0;
                    }
                }
                flags &= 7;


                var q = new Quaternion();
                float sum = 0;
                for (int j = 0; j < 4; j++)
                {
                    if ((flags & 3) != j)
                    {
                        int bitSize = ((flags & 3) + 1) % 4 == j ? 9 : 10;
                        uint x = 0;

                        bits = 0;
                        while (bits < bitSize)
                        {
                            x |= (uint)(res.Data[indexPos] >> bitPos << bits);
                            int num = Math.Min(bitSize - bits, 8 - bitPos);
                            bitPos += num;
                            bits += num;
                            if (bitPos == 8)
                            {
                                indexPos++;
                                bitPos = 0;
                            }
                        }
                        x &= (uint)((1 << bitSize) - 1);
                        q[j] = x / (0.5f * ((1 << bitSize) - 1)) - 1;
                        sum += q[j] * q[j];
                    }
                }

                int lastComponent = (int)(flags & 3);
                q[lastComponent] = (float)Math.Sqrt(1 - sum);
                if ((flags & 4) != 0u)
                    q[lastComponent] = -q[lastComponent];
                data[i] = q;
            }

            return data;
        }
    }

}
