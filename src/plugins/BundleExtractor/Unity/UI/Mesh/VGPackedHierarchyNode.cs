using System.Numerics;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class VGPackedHierarchyNode
    {
        public Vector4[] LODBounds = new Vector4[8];
        public Vector3[] BoxBoundsCenter = new Vector3[8];
        public uint[] MinLODError_MaxParentLODError = new uint[8];
        public Vector3[] BoxBoundsExtent = new Vector3[8];
        public uint[] ChildStartReference = new uint[8];
        public uint[] ResourcePageIndex_NumPages_GroupPartSize = new uint[8];

        public VGPackedHierarchyNode(IBundleBinaryReader reader)
        {
            for (int i = 0; i < 8; i++)
            {
                LODBounds[i] = reader.ReadVector4();
                BoxBoundsCenter[i] = reader.ReadVector3();
                MinLODError_MaxParentLODError[i] = reader.ReadUInt32();
                BoxBoundsExtent[i] = reader.ReadVector3();
                ChildStartReference[i] = reader.ReadUInt32();
                ResourcePageIndex_NumPages_GroupPartSize[i] = reader.ReadUInt32();
            }
        }
    }
}
