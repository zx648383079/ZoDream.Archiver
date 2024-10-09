using System.Collections.Generic;

namespace ZoDream.BundleExtractor.UI
{
    public sealed class Animation : UIBehaviour
    {
        public List<PPtr<AnimationClip>> m_Animations;

        public Animation(UIReader reader) 
            : base(reader)
        {
            var m_Animation = new PPtr<AnimationClip>(reader);
            int numAnimations = reader.Reader.ReadInt32();
            m_Animations = new List<PPtr<AnimationClip>>();
            for (int i = 0; i < numAnimations; i++)
            {
                m_Animations.Add(new PPtr<AnimationClip>(reader));
            }
        }
    }
}
