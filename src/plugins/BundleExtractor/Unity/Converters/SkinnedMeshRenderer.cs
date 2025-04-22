using System;
using UnityEngine;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
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
            if (reader.TryGet<IDependencyBuilder>(out var builder))
            {
                var container = reader.Get<ISerializedFile>();
                var fileName = container.FullPath;
                var fileId = reader.Get<ObjectInfo>().FileID;
                builder.AddDependencyEntry(fileName,
                    fileId,
                    res.Mesh.PathID);
                if (res.RootBone is not null)
                {
                    builder.AddDependencyEntry(fileName,
                        fileId,
                        res.RootBone.PathID);
                }
                foreach (var item in res.Bones)
                {
                    builder.AddDependencyEntry(fileName,
                        fileId,
                        item.PathID);
                }
            }

            return res;
        }

    }
}
