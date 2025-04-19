
namespace UnityEngine
{
    public struct ConstantBuffer
    {
        public int NameIndex;
        public MatrixParameter[] MatrixParams;
        public VectorParameter[] VectorParams;
        public StructParameter[] StructParams;
        public int Size;
        public bool IsPartialCB;

    }

}
