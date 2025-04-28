using System;
using UnityEngine;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal sealed class MeshRendererConverter : BundleConverter<MeshRenderer>
    {
        public override MeshRenderer? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new MeshRenderer();

            RendererConverter.Read(res, reader, serializer);

            res.AdditionalVertexStreams = reader.ReadPPtr<Mesh>(serializer);
            if (reader.TryGet<IDependencyBuilder>(out var builder))
            {
                var container = reader.Get<ISerializedFile>();
                var fileName = container.FullPath;
                var fileId = reader.Get<ObjectInfo>().FileID;
                builder.AddDependencyEntry(fileName,
                    fileId,
                    res.GameObject.PathID);
                builder.AddDependencyEntry(fileName,
                    fileId,
                    res.AdditionalVertexStreams.PathID);
            }
            return res;
        }
    }
}
