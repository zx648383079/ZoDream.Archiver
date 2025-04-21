using System;
using UnityEngine;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class LimitConverter : BundleConverter<Limit>
    {
        public override Limit? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new Limit();
            var version = reader.Get<Version>();
            if (version.GreaterThanOrEquals(5, 4))//5.4 and up
            {
                res.Min = reader.ReadVector3Or4();
                res.Max = reader.ReadVector3Or4();
            }
            else
            {
                res.Min = reader.ReadVector4();
                res.Max = reader.ReadVector4();
            }
            return res;
        }
    }

}
