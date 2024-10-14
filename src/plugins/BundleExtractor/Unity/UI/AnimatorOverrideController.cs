using System.Collections.Generic;

namespace ZoDream.BundleExtractor.Unity.UI
{
    public class AnimationClipOverride
    {
        public PPtr<AnimationClip> m_OriginalClip;
        public PPtr<AnimationClip> m_OverrideClip;

        public AnimationClipOverride(UIReader reader)
        {
            m_OriginalClip = new PPtr<AnimationClip>(reader);
            m_OverrideClip = new PPtr<AnimationClip>(reader);
        }
    }

    public sealed class AnimatorOverrideController : RuntimeAnimatorController
    {
        public PPtr<RuntimeAnimatorController> m_Controller;
        public List<AnimationClipOverride> m_Clips;

        public AnimatorOverrideController(UIReader reader) : base(reader)
        {
            m_Controller = new PPtr<RuntimeAnimatorController>(reader);

            int numOverrides = reader.Reader.ReadInt32();
            m_Clips = new List<AnimationClipOverride>();
            for (int i = 0; i < numOverrides; i++)
            {
                m_Clips.Add(new AnimationClipOverride(reader));
            }
        }
    }
}
