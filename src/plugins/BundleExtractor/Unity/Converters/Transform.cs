using System;
using UnityEngine;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal sealed class TransformConverter : BundleConverter<Transform>
    {
        public override Transform? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var target = reader.Get<BuildTarget>();
            var version = reader.Get<Version>();
            if (target == BuildTarget.NoTarget)
            {
                var m_ObjectHideFlags = reader.ReadUInt32();
                var m_PrefabParentObject = serializer.Deserialize<PPtr>(reader);
                var m_PrefabInternal = serializer.Deserialize<PPtr>(reader);
            }
            var res = new Transform();
            res.GameObject = serializer.Deserialize<PPtr<GameObject>>(reader);
            res.LocalRotation = reader.ReadQuaternion();
            res.LocalPosition = reader.ReadVector3Or4();
            res.LocalScale = reader.ReadVector3Or4();

            res.Children = reader.ReadArray(_ => serializer.Deserialize<PPtr<Transform>>(reader));
            res.Father = serializer.Deserialize<PPtr<Transform>>(reader);

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
                    res.Father.PathID);
                foreach (var item in res.Children)
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
