using System;
using UnityEngine;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class StateConstantConverter : BundleConverter<StateConstant>
    {
        public override StateConstant? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new StateConstant();
            ReadBase(res, reader, serializer, () => { });

            reader.AlignStream();
            return res;
        }

        public static void ReadBase(StateConstant res, IBundleBinaryReader reader, IBundleSerializer serializer, Action cb)
        {
            var version = reader.Get<Version>();

            res.TransitionConstantArray = reader.ReadArray<TransitionConstant>(serializer);

            res.BlendTreeConstantIndexArray = reader.ReadArray(r => r.ReadInt32());

            if (version.LessThan(5, 2)) //5.2 down
            {
                res.LeafInfoArray = reader.ReadArray<LeafInfoConstant>(serializer);
            }

            res.BlendTreeConstantArray = reader.ReadArray<BlendTreeConstant>(serializer);

            res.NameID = reader.ReadUInt32();
            if (version.GreaterThanOrEquals(4, 3)) //4.3 and up
            {
                res.PathID = reader.ReadUInt32();
            }
            if (version.GreaterThanOrEquals(5)) //5.0 and up
            {
                res.FullPathID = reader.ReadUInt32();
            }

            res.TagID = reader.ReadUInt32();
            if (version.GreaterThanOrEquals(5, 1)) //5.1 and up
            {
                res.SpeedParamID = reader.ReadUInt32();
                res.MirrorParamID = reader.ReadUInt32();
                res.CycleOffsetParamID = reader.ReadUInt32();
            }

            if (version.GreaterThanOrEquals(2017, 2)) //2017.2 and up
            {
                var m_TimeParamID = reader.ReadUInt32();
            }

            res.Speed = reader.ReadSingle();
            if (version.GreaterThanOrEquals(4, 1)) //4.1 and up
            {
                res.CycleOffset = reader.ReadSingle();
            }
            res.IKOnFeet = reader.ReadBoolean();
            if (version.GreaterThanOrEquals(5)) //5.0 and up
            {
                res.WriteDefaultValues = reader.ReadBoolean();
            }

            res.Loop = reader.ReadBoolean();
            if (version.GreaterThanOrEquals(4, 1)) //4.1 and up
            {
                res.Mirror = reader.ReadBoolean();
            }

            
        }
    }
}
