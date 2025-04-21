using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class SkeletonMaskElementConverter : BundleConverter<SkeletonMaskElement>
    {
        public override SkeletonMaskElement Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new()
            {
                PathHash = reader.ReadUInt32(),
                Weight = reader.ReadSingle()
            };
        }
    }
}
