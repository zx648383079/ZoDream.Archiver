using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class BoneWeights4
    {
        public float[] weight;
        public int[] boneIndex;

        public BoneWeights4()
        {
            weight = new float[4];
            boneIndex = new int[4];
        }

        public BoneWeights4(UIReader reader)
        {
            weight = reader.ReadArray(4, r => r.ReadSingle());
            boneIndex = reader.ReadArray(4, r => r.ReadInt32());
        }
    }

}
