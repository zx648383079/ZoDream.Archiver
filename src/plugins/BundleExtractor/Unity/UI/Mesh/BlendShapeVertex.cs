using System.Numerics;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class BlendShapeVertex
    {
        public Vector3 vertex;
        public Vector3 normal;
        public Vector3 tangent;
        public uint index;

        public BlendShapeVertex() { }

        public BlendShapeVertex(IBundleBinaryReader reader)
        {
            vertex = reader.ReadVector3Or4();
            normal = reader.ReadVector3Or4();
            tangent = reader.ReadVector3Or4();
            index = reader.ReadUInt32();
        }
    }

}
