using System;
using UnityEngine;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal sealed class AnimationConverter : BundleConverter<Animation>
    {
        public override Animation? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var target = reader.Get<BuildTarget>();
            if (target == BuildTarget.NoTarget)
            {
                var m_ObjectHideFlags = reader.ReadUInt32();
                var m_PrefabParentObject = serializer.Deserialize<PPtr>(reader);
                var m_PrefabInternal = serializer.Deserialize<PPtr>(reader);
            }
            var res = new Animation
            {
                GameObject = serializer.Deserialize<PPtr<GameObject>>(reader),
                IsEnabled = reader.ReadByte()
            };
            reader.AlignStream();
            res.Clip = serializer.Deserialize<PPtr<AnimationClip>>(reader);
            res.Clips = reader.ReadArray(_ => serializer.Deserialize<PPtr<AnimationClip>>(reader)!);
            if (reader.TryGet<IDependencyBuilder>(out var builder))
            {
                var container = reader.Get<ISerializedFile>();
                var fileName = container.FullPath;
                var fileId = reader.Get<ObjectInfo>().FileID;
                builder.AddDependencyEntry(fileName,
                    fileId,
                    res.GameObject!.PathID);
                builder.AddDependencyEntry(fileName,
                    fileId,
                    res.Clip!.PathID);
                foreach (var item in res.Clips)
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
