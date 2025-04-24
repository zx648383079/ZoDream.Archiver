using System;
using UnityEngine;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class BlendShapeDataConverter : BundleConverter<BlendShapeData>
    {
        public static bool HasVarintVertices(SerializedType type) => Convert.ToHexString(type.OldTypeHash) == "70AE601CDF0C273E745D9EC1333426A4";

        public static void ReadBase(BlendShapeData res, IBundleBinaryReader reader, 
            IBundleSerializer serializer, Action cb)
        {
            var version = reader.Get<Version>();

            if (version.GreaterThanOrEquals(4, 3)) //4.3 and up
            {
                res.Vertices = reader.ReadArray<BlendShapeVertex>(serializer);
                res.Shapes = reader.ReadArray<MeshBlendShape>(serializer);
            }
            else
            {
                res.Shapes = reader.ReadArray<MeshBlendShape>(serializer);
                reader.AlignStream();
                res.Vertices = reader.ReadArray<BlendShapeVertex>(serializer); //MeshBlendShapeVertex
            }
            cb.Invoke();
        }

        public override BlendShapeData? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new BlendShapeData();
            ReadBase(res, reader, serializer, () => { });
            var version = reader.Get<Version>();

            if (version.GreaterThanOrEquals(4, 3)) //4.3 and up
            {
                res.Channels = reader.ReadArray<MeshBlendShapeChannel>(serializer);

                res.FullWeights = reader.ReadArray(r => r.ReadSingle());

            }
            return res;
        }
    }

}
