using System.Collections.Generic;

namespace UnityEngine
{
    public sealed class AnimatorController : RuntimeAnimatorController
    {
        public Dictionary<uint, string> TOS;
        public PPtr<AnimationClip>[] AnimationClips;
    }
}
