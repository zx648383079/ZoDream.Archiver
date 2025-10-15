using System.Numerics;

namespace UnityEngine
{
    public class ValueArray
    {
        public bool[] BoolValues { get; set; }
        public int[] IntValues { get; set; }
        public float[] FloatValues { get; set; }
        public Vector4[] VectorValues { get; set; }
        public Vector3[] PositionValues { get; set; }
        public Vector4[] QuaternionValues { get; set; }
        public Vector3[] ScaleValues { get; set; }
        public int[] EntityIdValues { get; set; }
    }

}
