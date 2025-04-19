namespace UnityEngine
{
    public class AnimationCurve<T>
    {
        public Keyframe<T>[] Curve {  get; set; }
        public int PreInfinity { get; set; }
        public int PostInfinity { get; set; }
        public int RotationOrder { get; set; }

    }

}
