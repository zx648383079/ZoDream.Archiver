using System.Numerics;

namespace UnityEngine
{
    public class Blend2dDataConstant
    {
        public Vector2[] ChildPositionArray {  get; set; }
        public float[] ChildMagnitudeArray { get; set; }
        public Vector2[] ChildPairVectorArray { get; set; }
        public float[] ChildPairAvgMagInvArray { get; set; }
        public MotionNeighborList[] ChildNeighborListArray { get; set; }
    }
}
