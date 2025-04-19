using System.Numerics;

namespace UnityEngine
{
    public class VGPackedHierarchyNode
    {
        public Vector4[] LODBounds = new Vector4[8];
        public Vector3[] BoxBoundsCenter = new Vector3[8];
        public uint[] MinLODError_MaxParentLODError = new uint[8];
        public Vector3[] BoxBoundsExtent = new Vector3[8];
        public uint[] ChildStartReference = new uint[8];
        public uint[] ResourcePageIndex_NumPages_GroupPartSize = new uint[8];

    }
}
