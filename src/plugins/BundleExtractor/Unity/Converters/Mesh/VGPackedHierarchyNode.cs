using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class VGPackedHierarchyNodeConverter : BundleConverter<VGPackedHierarchyNode>
    {
        public override VGPackedHierarchyNode? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new VGPackedHierarchyNode();
            for (int i = 0; i < 8; i++)
            {
                res.LODBounds[i] = reader.ReadVector4();
                res.BoxBoundsCenter[i] = reader.ReadVector3();
                res.MinLODError_MaxParentLODError[i] = reader.ReadUInt32();
                res.BoxBoundsExtent[i] = reader.ReadVector3();
                res.ChildStartReference[i] = reader.ReadUInt32();
                res.ResourcePageIndex_NumPages_GroupPartSize[i] = reader.ReadUInt32();
            }
            return res;
        }
    }
}
