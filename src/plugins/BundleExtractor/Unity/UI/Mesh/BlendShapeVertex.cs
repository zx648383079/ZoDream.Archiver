using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class BlendShapeVertex
    {
        public Vector3 vertex;
        public Vector3 normal;
        public Vector3 tangent;
        public uint index;

        public BlendShapeVertex() { }

        public BlendShapeVertex(UIReader reader)
        {
            vertex = reader.ReadVector3();
            normal = reader.ReadVector3();
            tangent = reader.ReadVector3();
            index = reader.ReadUInt32();
        }
    }

}
