using System.Collections.Generic;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{

    internal sealed class AnimatorController(UIReader reader) : 
        RuntimeAnimatorController(reader)
    {
        public Dictionary<uint, string> m_TOS;
        public List<PPtr<AnimationClip>> m_AnimationClips;

        public override void Read(IBundleBinaryReader reader)
        {
            base.Read(reader);
            var m_ControllerSize = reader.ReadUInt32();
            var m_Controller = new ControllerConstant(_reader);

            int tosSize = reader.ReadInt32();
            m_TOS = new Dictionary<uint, string>();
            for (int i = 0; i < tosSize; i++)
            {
                m_TOS.Add(reader.ReadUInt32(), reader.ReadAlignedString());
            }

            int numClips = reader.ReadInt32();

            m_AnimationClips = [];
            for (int i = 0; i < numClips; i++)
            {
                m_AnimationClips.Add(new PPtr<AnimationClip>(_reader));
            }
        }
    }
}
