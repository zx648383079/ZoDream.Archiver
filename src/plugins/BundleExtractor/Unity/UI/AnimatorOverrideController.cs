using System.Collections.Generic;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{

    internal sealed class AnimatorOverrideController(UIReader reader) : 
        RuntimeAnimatorController(reader)
    {
        public PPtr<RuntimeAnimatorController> m_Controller;
        public List<AnimationClipOverride> m_Clips;

        public override void Read(IBundleBinaryReader reader)
        {
            base.Read(reader);
            m_Controller = new PPtr<RuntimeAnimatorController>(_reader);

            int numOverrides = reader.ReadInt32();
            m_Clips = [];
            for (int i = 0; i < numOverrides; i++)
            {
                m_Clips.Add(new AnimationClipOverride(_reader));
            }
        }
    }
}
