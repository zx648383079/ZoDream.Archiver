using System.Collections.Generic;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal sealed class Animation(UIReader reader) : UIBehavior(reader)
    {
        public List<PPtr<AnimationClip>> m_Animations;

        public override void Read(IBundleBinaryReader reader)
        {
            base.Read(reader);
            var m_Animation = new PPtr<AnimationClip>(_reader);
            int numAnimations = reader.ReadInt32();
            m_Animations = [];
            for (int i = 0; i < numAnimations; i++)
            {
                m_Animations.Add(new PPtr<AnimationClip>(_reader));
            }
        }

        public override void Associated(IDependencyBuilder? builder)
        {
            base.Associated(builder);
            foreach (var item in m_Animations)
            {
                builder?.AddDependencyEntry(_reader.FullPath, FileID, item.m_PathID);
            }
        }
    }
}
