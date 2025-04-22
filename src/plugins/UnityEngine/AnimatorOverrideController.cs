
namespace UnityEngine
{
    public sealed class AnimatorOverrideController : RuntimeAnimatorController
    {
        public IPPtr<RuntimeAnimatorController> Controller;
        public AnimationClipOverride[] Clips;
    }
}
