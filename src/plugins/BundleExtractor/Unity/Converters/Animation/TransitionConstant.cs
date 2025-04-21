using System;
using UnityEngine;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class TransitionConstantConverter : BundleConverter<TransitionConstant>
    {
        public override TransitionConstant? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var version = reader.Get<Version>();
            var res = new TransitionConstant();
            res.ConditionConstantArray = reader.ReadArray<ConditionConstant>(serializer);

            res.DestinationState = reader.ReadUInt32();
            if (version.GreaterThanOrEquals(5)) //5.0 and up
            {
                res.FullPathID = reader.ReadUInt32();
            }

            res.ID = reader.ReadUInt32();
            res.UserID = reader.ReadUInt32();
            res.TransitionDuration = reader.ReadSingle();
            res.TransitionOffset = reader.ReadSingle();
            if (version.GreaterThanOrEquals(5)) //5.0 and up
            {
                res.ExitTime = reader.ReadSingle();
                res.HasExitTime = reader.ReadBoolean();
                res.HasFixedDuration = reader.ReadBoolean();
                reader.AlignStream();
                res.InterruptionSource = reader.ReadInt32();
                res.OrderedInterruption = reader.ReadBoolean();
            }
            else
            {
                res.Atomic = reader.ReadBoolean();
            }

            if (version.GreaterThanOrEquals(4, 5)) //4.5 and up
            {
                res.CanTransitionToSelf = reader.ReadBoolean();
            }

            reader.AlignStream();
            return res;
        }
    }
}
