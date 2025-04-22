namespace UnityEngine
{
    public class AnimationClipOverride
    {
        public IPPtr<AnimationClip> OriginalClip { get; set; }
        public IPPtr<AnimationClip> OverrideClip { get; set; }

    }
}
