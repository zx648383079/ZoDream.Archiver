using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class MeshBlendShapeChannel
    {
        public string name;
        public uint nameHash;
        public int frameIndex;
        public int frameCount;

        public MeshBlendShapeChannel(UIReader reader)
        {
            name = reader.ReadAlignedString();
            nameHash = reader.ReadUInt32();
            frameIndex = reader.ReadInt32();
            frameCount = reader.ReadInt32();
        }
    }

}
