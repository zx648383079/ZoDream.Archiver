namespace UnityEngine
{
    public sealed class Animation : Behaviour
    {
        public WrapMode WrapMode {  get; set; }

        public Bounds LocalBounds { get; set; }

        public IPPtr<AnimationClip> Clip { get; set; }
        public IPPtr<AnimationClip>[] Clips { get; set; }
    }
}
