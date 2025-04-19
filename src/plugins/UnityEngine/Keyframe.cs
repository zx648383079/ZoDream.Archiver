namespace UnityEngine
{
    public struct Keyframe<T>
    {
        public float Time { get; set; }
        public T Value { get; set; }
        public T InSlope { get; set; }
        public T OutSlope { get; set; }
        public int WeightedMode { get; set; }
        public T InWeight { get; set; }
        public T OutWeight { get; set; }
    }
}
