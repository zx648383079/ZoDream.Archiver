using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class Blend1dDataConstantConverter : BundleConverter<Blend1dDataConstant>
    {
        public override Blend1dDataConstant? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new()
            {
                ChildThresholdArray = reader.ReadArray(r => r.ReadSingle())
            };
        }
    }
}
