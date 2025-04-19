
namespace UnityEngine
{
    public class CompressedAnimationCurve
    {
        public string Path { get; set; }
        public PackedIntVector Times { get; set; }
        public PackedQuatVector Values { get; set; }
        public PackedFloatVector Slopes { get; set; }
        public int PreInfinity { get; set; }
        public int PostInfinity { get; set; }
    }

}
