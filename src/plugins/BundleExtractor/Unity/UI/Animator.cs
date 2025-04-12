using System;
using ZoDream.BundleExtractor.Models;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal sealed class Animator(UIReader reader) : UIBehavior(reader), IFileExporter
    {
        public PPtr<Avatar> m_Avatar;
        public PPtr<RuntimeAnimatorController> m_Controller;
        public bool m_HasTransformHierarchy = true;

        public override string Name => m_GameObject.Name;


        public override void Read(IBundleBinaryReader reader)
        {
            var version = reader.Get<UnityVersion>();
            ReadBase(reader, () => {
                var m_CullingMode = reader.ReadInt32();

                if (version.GreaterThanOrEquals(4, 5)) //4.5 and up
                {
                    var m_UpdateMode = reader.ReadInt32();
                }
            });
        }

        public void ReadBase(IBundleBinaryReader reader, Action cb)
        {
            base.Read(reader);
            var version = reader.Get<UnityVersion>();
            m_Avatar = new PPtr<Avatar>(reader);
            m_Controller = new PPtr<RuntimeAnimatorController>(reader);
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
                m_HasTransformHierarchy = reader.ReadBoolean();
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
        public override void Associated(IDependencyBuilder? builder)
        {
            base.Associated(builder);
            builder?.AddDependencyEntry(_reader.FullPath, FileID, m_Avatar.m_PathID);
            builder?.AddDependencyEntry(_reader.FullPath, FileID, m_Controller.m_PathID);
        }
        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            // TODO
        }
    }
}
