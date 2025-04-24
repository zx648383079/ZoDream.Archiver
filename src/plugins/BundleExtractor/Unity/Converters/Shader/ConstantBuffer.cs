using System;
using UnityEngine;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class ConstantBufferConverter : BundleConverter<ConstantBuffer>
    {
        public override ConstantBuffer Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new ConstantBuffer();
            ReadBase(ref res, reader, serializer, () => { });
            return res;
        }

        public static void ReadBase(ref ConstantBuffer res, IBundleBinaryReader reader, IBundleSerializer serializer, Action cb)
        {
            var version = reader.Get<Version>();

            res.NameIndex = reader.ReadInt32();

            res.MatrixParams = reader.ReadArray(_ => serializer.Deserialize<MatrixParameter>(reader));
            res.VectorParams = reader.ReadArray(_ => serializer.Deserialize<VectorParameter>(reader));

            if (version.GreaterThanOrEquals(2017, 3)) //2017.3 and up
            {
                res.StructParams = reader.ReadArray(_ => serializer.Deserialize<StructParameter>(reader));
            }
            res.Size = reader.ReadInt32();

            if (version.GreaterThanOrEquals(2020, 3, 2, VersionType.Final, 1) || //2020.3.2f1 and up
              version.GreaterThanOrEquals(2021, 1, 4, VersionType.Final, 1)) //2021.1.4f1 and up
            {
                cb.Invoke();
                res.IsPartialCB = reader.ReadBoolean();
                reader.AlignStream();
            }
        }
    }

}
