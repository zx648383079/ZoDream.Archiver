namespace UnityEngine
{
    public class ACLDenseClip : DenseClip
    {
        public int ACLType { get; set; }
        public byte[] ACLArray { get; set; }
        public float PositionFactor { get; set; }
        public float EulerFactor { get; set; }
        public float ScaleFactor { get; set; }
        public float FloatFactor { get; set; }
        public uint nPositionCurves { get; set; }
        public uint nRotationCurves { get; set; }
        public uint nEulerCurves { get; set; }
        public uint nScaleCurves { get; set; }
        public uint nGenericCurves { get; set; }

    }
}
