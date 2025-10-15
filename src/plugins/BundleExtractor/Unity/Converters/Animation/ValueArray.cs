using System;
using UnityEngine;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class ValueArrayConverter : BundleConverter<ValueArray>
    {
        public override ValueArray? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var version = reader.Get<Version>();
            var res = new ValueArray();
            if (version.LessThan(5, 5)) //5.5 down
            {
                res.BoolValues = reader.ReadArray(r => r.ReadBoolean());
                reader.AlignStream();
                res.IntValues = reader.ReadInt32Array();
                res.FloatValues = reader.ReadArray(r => r.ReadSingle());
            }

            if (version.GreaterThan(4, 3)) //4.3 down
            {
                res.VectorValues = reader.ReadArray(_ => reader.ReadVector4());
            }
            else
            {
                res.PositionValues = reader.ReadVector3Array();

                res.QuaternionValues = reader.ReadArray(_ => reader.ReadVector4());

                res.ScaleValues = reader.ReadVector3Array();

                if (version.GreaterThanOrEquals(5, 5)) //5.5 and up
                {
                    res.FloatValues = reader.ReadArray(r => r.ReadSingle());
                    res.IntValues = reader.ReadInt32Array();
                    res.BoolValues = reader.ReadArray(r => r.ReadBoolean());
                    reader.AlignStream();
                }
                if (version.GreaterThanOrEquals(6000, 2))
                {
                    res.EntityIdValues = reader.ReadInt32Array();
                }
            }
            return res;
        }
    }

}
