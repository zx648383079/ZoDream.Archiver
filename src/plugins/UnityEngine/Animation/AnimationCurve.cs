namespace UnityEngine
{
    public class AnimationCurve<T>
    {
        public Keyframe<T>[] Curve {  get; set; }
        public int PreInfinity { get; set; } = 2;
        public int PostInfinity { get; set; } = 2;
        public int RotationOrder { get; set; } = 4;

    }

}
