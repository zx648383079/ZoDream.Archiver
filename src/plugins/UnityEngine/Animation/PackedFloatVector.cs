namespace UnityEngine
{
    public struct PackedFloatVector
    {
        public uint NumItems { get; set; }
        public float Range { get; set; }
        public float Start { get; set; }
        public byte[] Data { get; set; }
        public byte BitSize { get; set; }

    }

}
