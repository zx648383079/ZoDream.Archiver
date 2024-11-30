using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class SubMesh
    {
        public uint firstByte;
        public uint indexCount;
        public GfxPrimitiveType topology;
        public uint triangleCount;
        public uint baseVertex;
        public uint firstVertex;
        public uint vertexCount;
        public AABB localAABB;

        public SubMesh(UIReader reader)
        {
            var version = reader.Version;

            firstByte = reader.ReadUInt32();
            indexCount = reader.ReadUInt32();
            topology = (GfxPrimitiveType)reader.ReadInt32();

            if (version.LessThan(4)) //4.0 down
            {
                triangleCount = reader.ReadUInt32();
            }

            if (version.GreaterThanOrEquals(2017, 3)) //2017.3 and up
            {
                baseVertex = reader.ReadUInt32();
            }

            if (version.GreaterThanOrEquals(3)) //3.0 and up
            {
                firstVertex = reader.ReadUInt32();
                vertexCount = reader.ReadUInt32();
                localAABB = new AABB(reader);
            }
        }
    }

}
