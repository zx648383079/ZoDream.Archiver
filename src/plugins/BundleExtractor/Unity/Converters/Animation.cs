using System;
using UnityEngine;
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
                GameObject = reader.ReadPPtr<GameObject>(serializer),
                IsEnabled = reader.ReadByte()
            };
            reader.AlignStream();
            res.Clip = reader.ReadPPtr<AnimationClip>(serializer);
            res.Clips = reader.ReadPPtrArray<AnimationClip>(serializer);
            return res; 
        }

    }
}
