using System;
using UnityEngine;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class AxesConverter : BundleConverter<Axes>
    {
        public override Axes? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var version = reader.Get<Version>();
            var res = new Axes();
            res.PreQ = reader.ReadVector4();
            res.PostQ = reader.ReadVector4();
            if (version.GreaterThanOrEquals(5, 4)) //5.4 and up
            {
                res.Sgn = reader.ReadVector3Or4();
            }
            else
            {
                res.Sgn = reader.ReadVector4();
            }
            res.Limit = serializer.Deserialize<Limit>(reader);
            res.Length = reader.ReadSingle();
            res.Type = reader.ReadUInt32();
            return res;
        }
    }

}
