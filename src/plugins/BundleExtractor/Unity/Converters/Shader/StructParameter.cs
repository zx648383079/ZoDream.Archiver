using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class StructParameterConverter : BundleConverter<StructParameter>
    {
        public override StructParameter Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new StructParameter();
            var m_NameIndex = reader.ReadInt32();
            var m_Index = reader.ReadInt32();
            var m_ArraySize = reader.ReadInt32();
            var m_StructSize = reader.ReadInt32();

            res.VectorParams = reader.ReadArray(_ => serializer.Deserialize<VectorParameter>(reader));
            res.MatrixParams = reader.ReadArray(_ => serializer.Deserialize<MatrixParameter>(reader));

            return res;
        }
    }

}
