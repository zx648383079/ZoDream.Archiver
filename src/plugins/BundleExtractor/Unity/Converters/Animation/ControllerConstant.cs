using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class ControllerConstantConverter : BundleConverter<ControllerConstant>
    {
        public override ControllerConstant? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new ControllerConstant
            {
                LayerArray = reader.ReadArray<LayerConstant>(serializer),
                StateMachineArray = reader.ReadArray<StateMachineConstant>(serializer),

                Values = serializer.Deserialize<ValueArrayConstant>(reader),
                DefaultValues = serializer.Deserialize<ValueArray>(reader)
            };
            return res;
        }
    }
}
