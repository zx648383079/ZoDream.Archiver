using System;
using UnityEngine;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal sealed class SkinnedMeshRendererConverter : BundleConverter<SkinnedMeshRenderer>
    {
        public override SkinnedMeshRenderer? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new SkinnedMeshRenderer();
            RendererConverter.Read(res, reader, serializer);
            var version = reader.Get<Version>();
            int m_Quality = reader.ReadInt32();
            var m_UpdateWhenOffscreen = reader.ReadBoolean();
            var m_SkinNormals = reader.ReadBoolean(); //3.1.0 and below
            reader.AlignStream();

            if (version.LessThan(2, 6)) //2.6 down
            {
                var m_DisableAnimationWhenOffscreen = reader.ReadPPtr<Animation>(serializer);
            }

            res.Mesh = reader.ReadPPtr<Mesh>(serializer);

            res.Bones = reader.ReadArray(_ => reader.ReadPPtr<Transform>(serializer));

            if (version.GreaterThanOrEquals(4, 3)) //4.3 and up
            {
                res.BlendShapeWeights = reader.ReadArray(r => r.ReadSingle());
            }
            return res;
        }

    }
}
