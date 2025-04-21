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
            var target = reader.Get<BuildTarget>();
            if (target == BuildTarget.NoTarget)
            {
                var m_ObjectHideFlags = reader.ReadUInt32();
                var m_PrefabParentObject = serializer.Deserialize<PPtr>(reader);
                var m_PrefabInternal = serializer.Deserialize<PPtr>(reader);
            }
            var res = new MeshRenderer();
            res.GameObject = serializer.Deserialize<PPtr<GameObject>>(reader);
            res.AdditionalVertexStreams = serializer.Deserialize<PPtr<Mesh>>(reader);
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
