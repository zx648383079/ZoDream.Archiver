using System;
using UnityEngine;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class PPtrCurveConverter : BundleConverter<PPtrCurve>
    {
        public override PPtrCurve? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var version = reader.Get<Version>();
            var res = new PPtrCurve
            {
                Curve = reader.ReadArray<PPtrKeyframe>(serializer),

                Attribute = reader.ReadAlignedString(),
                Path = reader.ReadAlignedString(),
                ClassID = reader.ReadInt32(),
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
