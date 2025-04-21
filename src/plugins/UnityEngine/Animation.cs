namespace UnityEngine
{
    public sealed class Animation : Behaviour
    {
        public WrapMode WrapMode {  get; set; }

        public Bounds LocalBounds { get; set; }

        public PPtr<AnimationClip> Clip { get; set; }
        public PPtr<AnimationClip>[] Clips { get; set; }
    }
}
