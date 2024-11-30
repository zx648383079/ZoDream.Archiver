namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class AnimationClipOverride
    {
        public PPtr<AnimationClip> m_OriginalClip;
        public PPtr<AnimationClip> m_OverrideClip;

        public AnimationClipOverride(UIReader reader)
        {
            m_OriginalClip = new PPtr<AnimationClip>(reader);
            m_OverrideClip = new PPtr<AnimationClip>(reader);
        }
    }
}
