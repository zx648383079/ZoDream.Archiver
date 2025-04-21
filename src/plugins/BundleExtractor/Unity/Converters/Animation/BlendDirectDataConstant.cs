using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class BlendDirectDataConstantConverter : BundleConverter<BlendDirectDataConstant>
    {
        public override BlendDirectDataConstant? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new BlendDirectDataConstant
            {
                ChildBlendEventIDArray = reader.ReadArray(r => r.ReadUInt32()),
                NormalizedBlendValues = reader.ReadBoolean()
            };
            reader.AlignStream();
            return res;
        }
    }
}
