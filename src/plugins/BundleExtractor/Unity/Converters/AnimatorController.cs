using System;
using UnityEngine;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{

    internal sealed class AnimatorControllerConverter : BundleConverter<AnimatorController>
    {
        public override AnimatorController? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var target = reader.Get<BuildTarget>();
            var version = reader.Get<Version>();
            if (target == BuildTarget.NoTarget)
            {
                var m_ObjectHideFlags = reader.ReadUInt32();
                var m_PrefabParentObject = serializer.Deserialize<PPtr>(reader);
                var m_PrefabInternal = serializer.Deserialize<PPtr>(reader);
            }
            var res = new AnimatorController
            {
                Name = reader.ReadAlignedString()
            };
            var m_ControllerSize = reader.ReadUInt32();
            var m_Controller = serializer.Deserialize<ControllerConstant>(reader);

            int tosSize = reader.ReadInt32();
            res.TOS = [];
            for (int i = 0; i < tosSize; i++)
            {
                res.TOS.Add(reader.ReadUInt32(), reader.ReadAlignedString());
            }

            res.AnimationClips = reader.ReadPPtrArray<AnimationClip>(serializer);
            return res;
        }
    }
}
