using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class SkeletonPoseConverter : BundleConverter<SkeletonPose>
    {
        public override SkeletonPose? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new()
            {
                X = reader.ReadXFormArray()
            };
        }
    }

}
