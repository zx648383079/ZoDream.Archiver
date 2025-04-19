
namespace UnityEngine
{
    public sealed class AnimatorOverrideController : RuntimeAnimatorController
    {
        public PPtr<RuntimeAnimatorController> Controller;
        public AnimationClipOverride[] Clips;
    }
}
