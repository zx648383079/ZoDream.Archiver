using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class MeshBlendShape
    {
        public string name;
        public uint firstVertex;
        public uint vertexCount;
        public bool hasNormals;
        public bool hasTangents;

        public MeshBlendShape(UIReader reader)
        {
            var version = reader.Version;

            if (version.LessThan(4, 3)) //4.3 down
            {
                name = reader.ReadAlignedString();
            }
            firstVertex = reader.ReadUInt32();
            vertexCount = reader.ReadUInt32();
            if (version.LessThan(4, 3)) //4.3 down
            {
                var aabbMinDelta = reader.ReadVector3();
                var aabbMaxDelta = reader.ReadVector3();
            }
            hasNormals = reader.ReadBoolean();
            hasTangents = reader.ReadBoolean();
            if (!reader.IsLoveAndDeepSpace() && version.GreaterThanOrEquals(4, 3)) //4.3 and up
            {
                reader.AlignStream();
            }
        }
    }

}
