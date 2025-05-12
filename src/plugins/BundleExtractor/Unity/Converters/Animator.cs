using System;
using UnityEngine;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class AnimatorConverter : BundleConverter<Animator>
    {
        public override Animator? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var version = reader.Get<Version>();
            var res = new Animator();
            ReadBase(res, reader, serializer, () => {
                var m_CullingMode = reader.ReadInt32();

                if (version.GreaterThanOrEquals(4, 5)) //4.5 and up
                {
                    var m_UpdateMode = reader.ReadInt32();
                }
            });
            return res;
        }

        public static void ReadBase(Animator res, IBundleBinaryReader reader, IBundleSerializer serializer, Action cb)
        {
            var target = reader.Get<BuildTarget>();
            var version = reader.Get<Version>();
            if (target == BuildTarget.NoTarget)
            {
                var m_ObjectHideFlags = reader.ReadUInt32();
                var m_PrefabParentObject = serializer.Deserialize<PPtr>(reader);
                var m_PrefabInternal = serializer.Deserialize<PPtr>(reader);
            }
            res.GameObject = reader.ReadPPtr<GameObject>(serializer);
            res.IsEnabled = reader.ReadByte();
            reader.AlignStream();
            res.Avatar = reader.ReadPPtr<Avatar>(serializer);
            res.Controller = reader.ReadPPtr<RuntimeAnimatorController>(serializer);
            cb.Invoke();
            

            var m_ApplyRootMotion = reader.ReadBoolean();
            if (version.GreaterThanOrEquals(4, 5) && version.LessThan(5)) //4.5 and up - 5.0 down
            {
                reader.AlignStream();
            }

            if (version.GreaterThanOrEquals(5)) //5.0 and up
            {
                var m_LinearVelocityBlending = reader.ReadBoolean();
                if (version.GreaterThanOrEquals(2021, 2)) //2021.2 and up
                {
                    var m_StabilizeFeet = reader.ReadBoolean();
                }
                reader.AlignStream();
            }

            if (version.LessThan(4, 5)) //4.5 down
            {
                var m_AnimatePhysics = reader.ReadBoolean();
            }

            if (version.GreaterThanOrEquals(4, 3)) //4.3 and up
            {
                res.HasTransformHierarchy = reader.ReadBoolean();
            }

            if (version.GreaterThanOrEquals(4, 5)) //4.5 and up
            {
                var m_AllowConstantClipSamplingOptimization = reader.ReadBoolean();
            }
            if (version.GreaterThanOrEquals(5) && version.LessThan(2018)) //5.0 and up - 2018 down
            {
                reader.AlignStream();
            }

            if (version.GreaterThanOrEquals(2018)) //2018 and up
            {
                var m_KeepAnimatorControllerStateOnDisable = reader.ReadBoolean();
                reader.AlignStream();
            }
        }
    }
}
