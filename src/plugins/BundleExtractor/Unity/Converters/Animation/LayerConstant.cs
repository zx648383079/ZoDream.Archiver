using System;
using UnityEngine;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class LayerConstantConverter : BundleConverter<LayerConstant>
    {
        public override LayerConstant? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var version = reader.Get<Version>();
            var res = new LayerConstant();
            res.StateMachineIndex = reader.ReadUInt32();
            res.StateMachineMotionSetIndex = reader.ReadUInt32();
            res.BodyMask = serializer.Deserialize<HumanPoseMask>(reader);
            res.SkeletonMask = serializer.Deserialize<SkeletonMask>(reader);
            res.Binding = reader.ReadUInt32();
            res.LayerBlendingMode = reader.ReadInt32();
            if (version.GreaterThanOrEquals(4, 2)) //4.2 and up
            {
                res.DefaultWeight = reader.ReadSingle();
            }
            res.IKPass = reader.ReadBoolean();
            if (version.GreaterThanOrEquals(4, 2)) //4.2 and up
            {
                res.SyncedLayerAffectsTiming = reader.ReadBoolean();
            }
            reader.AlignStream();
            return res;
        }

    }
}
