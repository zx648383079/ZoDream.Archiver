using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class BoneWeights4Converter : BundleConverter<BoneWeights4>
    {
        public override BoneWeights4 Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new BoneWeights4
            {
                Weight = reader.ReadArray(4, (r, _) => r.ReadSingle()),
                BoneIndex = reader.ReadArray(4, (r, _) => r.ReadInt32())
            };
            return res;
        }
    }

}
