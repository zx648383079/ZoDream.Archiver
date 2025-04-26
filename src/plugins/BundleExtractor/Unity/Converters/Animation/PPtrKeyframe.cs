using System;
using UnityEngine;
using ZoDream.Shared.Bundle;
using Object = UnityEngine.Object;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class PPtrKeyframeConverter : BundleConverter<PPtrKeyframe>
    {
        public override PPtrKeyframe? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new()
            {
                Time = reader.ReadSingle(),
                Value = reader.ReadPPtr<Object>(serializer)
            };
        }

    }

}
