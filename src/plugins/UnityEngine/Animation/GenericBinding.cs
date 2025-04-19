namespace UnityEngine
{
    public class GenericBinding
    {
        public Version Version { get; set; }
        public uint Path { get; set; }
        public uint Attribute { get; set; }
        public PPtr<Object> Script { get; set; }
        public NativeClassID TypeID { get; set; }
        public byte CustomType { get; set; }
        public byte IsPPtrCurve { get; set; }
        public byte IsIntCurve { get; set; }
    }
}
