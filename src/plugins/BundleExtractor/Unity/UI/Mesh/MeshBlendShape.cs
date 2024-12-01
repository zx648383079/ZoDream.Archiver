using ZoDream.BundleExtractor.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class MeshBlendShape : IElementLoader
    {
        public string name;
        public uint firstVertex;
        public uint vertexCount;
        public bool hasNormals;
        public bool hasTangents;

        public void ReadBase(IBundleBinaryReader reader)
        {
            var version = reader.Get<UnityVersion>();

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
        }

        public void Read(IBundleBinaryReader reader)
        {
            ReadBase(reader);
            var version = reader.Get<UnityVersion>();
            if (version.GreaterThanOrEquals(4, 3)) //4.3 and up
            {
                reader.AlignStream();
            }
        }
    }

}
