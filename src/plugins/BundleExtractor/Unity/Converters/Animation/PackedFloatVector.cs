using System;
using System.Collections.Generic;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class PackedFloatVectorConverter : BundleConverter<PackedFloatVector>
    {
        public override PackedFloatVector Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new PackedFloatVector();
            res.NumItems = reader.ReadUInt32();

            res.Range = reader.ReadSingle();
            res.Start = reader.ReadSingle();
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

        public float[] UnpackFloats(PackedFloatVector res, int itemCountInChunk, int chunkStride, int start = 0, int numChunks = -1)
        {
            int bitPos = res.BitSize * start;
            int indexPos = bitPos / 8;
            bitPos %= 8;

            float scale = 1.0f / res.Range;
            if (numChunks == -1)
                numChunks = (int)res.NumItems / itemCountInChunk;
            var end = chunkStride * numChunks / 4;
            var data = new List<float>();
            for (var index = 0; index != end; index += chunkStride / 4)
            {
                for (int i = 0; i < itemCountInChunk; ++i)
                {
                    uint x = 0;

                    int bits = 0;
                    while (bits < res.BitSize)
                    {
                        x |= (uint)(res.Data[indexPos] >> bitPos << bits);
                        int num = Math.Min(res.BitSize - bits, 8 - bitPos);
                        bitPos += num;
                        bits += num;
                        if (bitPos == 8)
                        {
                            indexPos++;
                            bitPos = 0;
                        }
                    }
                    x &= (uint)(1 << res.BitSize) - 1u;
                    data.Add(x / (scale * ((1 << res.BitSize) - 1)) + res.Start);
                }
            }

            return [.. data];
        }
    }

}
