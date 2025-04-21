using System;
using UnityEngine;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class FloatCurveConverter : BundleConverter<FloatCurve>
    {
        public override FloatCurve? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var version = reader.Get<Version>();
            var res = new FloatCurve
            {
                Curve = UnityConverter.ReadAnimationCurve<float>(reader, reader.ReadSingle),
                Attribute = reader.ReadAlignedString(),
                Path = reader.ReadAlignedString(),
                ClassID = (NativeClassID)reader.ReadInt32(),
                Script = serializer.Deserialize<PPtr<MonoScript>>(reader)
            };
            if (version.GreaterThanOrEquals(2022, 2)) //2022.2 and up
            {
                res.Flags = reader.ReadInt32();
            }
            return res;
        }

    }

}
