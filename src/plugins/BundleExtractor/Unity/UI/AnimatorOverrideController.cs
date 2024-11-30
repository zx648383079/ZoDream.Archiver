using System.Collections.Generic;

namespace ZoDream.BundleExtractor.Unity.UI
{

    internal sealed class AnimatorOverrideController : RuntimeAnimatorController
    {
        public PPtr<RuntimeAnimatorController> m_Controller;
        public List<AnimationClipOverride> m_Clips;

        public AnimatorOverrideController(UIReader reader) : base(reader)
        {
            m_Controller = new PPtr<RuntimeAnimatorController>(reader);

            int numOverrides = reader.ReadInt32();
            m_Clips = new List<AnimationClipOverride>();
            for (int i = 0; i < numOverrides; i++)
            {
                m_Clips.Add(new AnimationClipOverride(reader));
            }
        }
    }
}
