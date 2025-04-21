using System;
using UnityEngine;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class StateMachineConstantConverter : BundleConverter<StateMachineConstant>
    {
        public override StateMachineConstant? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new StateMachineConstant();
            var version = reader.Get<Version>();
            res.StateConstantArray = reader.ReadArray<StateConstant>(serializer);
            res.AnyStateTransitionConstantArray = reader.ReadArray<TransitionConstant>(serializer);

            if (version.GreaterThanOrEquals(5)) //5.0 and up
            {
                res.SelectorStateConstantArray = reader.ReadArray<SelectorStateConstant>(serializer);
            }

            res.DefaultState = reader.ReadUInt32();
            res.MotionSetCount = reader.ReadUInt32();
            return res;
        }
    }

}
