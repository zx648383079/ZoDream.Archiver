using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class SkeletonMaskConverter : BundleConverter<SkeletonMask>
    {
        public override SkeletonMask? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new()
            {
                Data = reader.ReadArray<SkeletonMaskElement>(serializer)
            };
        }
    }
}
