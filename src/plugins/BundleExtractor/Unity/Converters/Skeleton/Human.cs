using System;
using UnityEngine;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class HumanConverter : BundleConverter<Human>
    {
        public override Human? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new Human();
            var version = reader.Get<Version>();
            res.RootX = reader.ReadXForm();
            res.Skeleton = serializer.Deserialize<Skeleton>(reader);
            res.SkeletonPose = serializer.Deserialize<SkeletonPose>(reader);
            res.LeftHand = serializer.Deserialize<Hand>(reader);
            res.RightHand = serializer.Deserialize<Hand>(reader);

            if (version.LessThan(2018, 2)) //2018.2 down
            {
                res.Handles = reader.ReadArray(_ => serializer.Deserialize<Handle>(reader));

                res.ColliderArray = reader.ReadArray(_ => serializer.Deserialize<Collider>(reader));
            }

            res.HumanBoneIndex = reader.ReadInt32Array();

            res.HumanBoneMass = reader.ReadArray(r => r.ReadSingle());

            if (version.LessThan(2018, 2)) //2018.2 down
            {
                res.ColliderIndex = reader.ReadArray(r => r.ReadInt32());
            }

            res.Scale = reader.ReadSingle();
            res.ArmTwist = reader.ReadSingle();
            res.ForeArmTwist = reader.ReadSingle();
            res.UpperLegTwist = reader.ReadSingle();
            res.LegTwist = reader.ReadSingle();
            res.ArmStretch = reader.ReadSingle();
            res.LegStretch = reader.ReadSingle();
            res.FeetSpacing = reader.ReadSingle();
            res.HasLeftHand = reader.ReadBoolean();
            res.HasRightHand = reader.ReadBoolean();
            if (version.GreaterThanOrEquals(5, 2)) //5.2 and up
            {
                res.HasTDoF = reader.ReadBoolean();
            }
            reader.AlignStream();
            return res;
        }
    }
}
