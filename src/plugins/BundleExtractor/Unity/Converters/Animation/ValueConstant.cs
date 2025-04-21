using System;
using UnityEngine;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class ValueConstantConverter : BundleConverter<ValueConstant>
    {
        public override ValueConstant? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var version = reader.Get<Version>();
            var res = new ValueConstant();
            res.ID = reader.ReadUInt32();
            if (version.LessThan(5, 5))//5.5 down
            {
                res.TypeID = reader.ReadUInt32();
            }
            res.Type = reader.ReadUInt32();
            res.Index = reader.ReadUInt32();
            return res;
        }
    }
}
