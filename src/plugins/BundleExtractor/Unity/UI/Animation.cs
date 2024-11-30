﻿using System.Collections.Generic;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal sealed class Animation : UIBehavior
    {
        public List<PPtr<AnimationClip>> m_Animations;

        public Animation(UIReader reader)
            : base(reader)
        {
            var m_Animation = new PPtr<AnimationClip>(reader);
            int numAnimations = reader.ReadInt32();
            m_Animations = [];
            for (int i = 0; i < numAnimations; i++)
            {
                m_Animations.Add(new PPtr<AnimationClip>(reader));
            }
        }
    }
}
