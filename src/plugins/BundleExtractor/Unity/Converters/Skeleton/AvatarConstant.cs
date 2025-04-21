using System;
using System.Numerics;
using UnityEngine;
using ZoDream.BundleExtractor.Models;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class AvatarConstantConverter : BundleConverter<AvatarConstant>
    {
        public override AvatarConstant? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var version = reader.Get<Version>();
            var res = new AvatarConstant();
            res.AvatarSkeleton = serializer.Deserialize<Skeleton>(reader);
            res.AvatarSkeletonPose = serializer.Deserialize<SkeletonPose>(reader);

            if (version.GreaterThanOrEquals(4, 3)) //4.3 and up
            {
                res.DefaultPose = serializer.Deserialize<SkeletonPose>(reader);

                res.SkeletonNameIDArray = reader.ReadArray(r => r.ReadUInt32());
            }

            res.Human = serializer.Deserialize<Human>(reader);

            res.HumanSkeletonIndexArray = reader.ReadArray(r => r.ReadInt32());

            if (version.GreaterThanOrEquals(4, 3)) //4.3 and up
            {
                res.HumanSkeletonReverseIndexArray = reader.ReadArray(r => r.ReadInt32());
            }

            res.RootMotionBoneIndex = reader.ReadInt32();
            res.RootMotionBoneX = reader.ReadXForm();

            if (version.GreaterThanOrEquals(4, 3)) //4.3 and up
            {
                res.RootMotionSkeleton = serializer.Deserialize<Skeleton>(reader);
                res.RootMotionSkeletonPose = serializer.Deserialize<SkeletonPose>(reader);

                res.RootMotionSkeletonIndexArray = reader.ReadArray(r => r.ReadInt32());
            }
            return res;
        }
    }

}
