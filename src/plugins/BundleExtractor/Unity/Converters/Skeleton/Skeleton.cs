using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class SkeletonConverter : BundleConverter<Skeleton>
    {
        public override Skeleton? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new Skeleton
            {
                Node = reader.ReadArray(_ => serializer.Deserialize<Node>(reader)),

                ID = reader.ReadArray(r => r.ReadUInt32()),

                AxesArray = reader.ReadArray(_ => serializer.Deserialize<Axes>(reader))
            };
        }
    }
}
