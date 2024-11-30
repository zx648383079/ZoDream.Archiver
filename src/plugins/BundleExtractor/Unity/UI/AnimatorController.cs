using System.Collections.Generic;

namespace ZoDream.BundleExtractor.Unity.UI
{

    internal sealed class AnimatorController : RuntimeAnimatorController
    {
        public Dictionary<uint, string> m_TOS;
        public List<PPtr<AnimationClip>> m_AnimationClips;

        public AnimatorController(UIReader reader) : base(reader)
        {
            var m_ControllerSize = reader.ReadUInt32();
            var m_Controller = new ControllerConstant(reader);

            int tosSize = reader.ReadInt32();
            m_TOS = new Dictionary<uint, string>();
            for (int i = 0; i < tosSize; i++)
            {
                m_TOS.Add(reader.ReadUInt32(), reader.ReadAlignedString());
            }

            int numClips = reader.ReadInt32();
            m_AnimationClips = new List<PPtr<AnimationClip>>();
            for (int i = 0; i < numClips; i++)
            {
                m_AnimationClips.Add(new PPtr<AnimationClip>(reader));
            }
        }
    }
}
