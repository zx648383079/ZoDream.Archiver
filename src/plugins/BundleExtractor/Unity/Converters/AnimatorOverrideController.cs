using System;
using UnityEngine;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{

    internal sealed class AnimatorOverrideControllerConverter : BundleConverter<AnimatorOverrideController>
    {
        public override AnimatorOverrideController? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var target = reader.Get<BuildTarget>();
            var version = reader.Get<Version>();
            if (target == BuildTarget.NoTarget)
            {
                var m_ObjectHideFlags = reader.ReadUInt32();
                var m_PrefabParentObject = serializer.Deserialize<PPtr>(reader);
                var m_PrefabInternal = serializer.Deserialize<PPtr>(reader);
            }
            var res = new AnimatorOverrideController();
            res.Name = reader.ReadAlignedString();
            res.Controller = reader.ReadPPtr<RuntimeAnimatorController>(serializer);

            res.Clips = reader.ReadArray<AnimationClipOverride>(serializer);
            return res;
        }
    }
}
