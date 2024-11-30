using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class SamplerParameter
    {
        public uint sampler;
        public int bindPoint;

        public SamplerParameter(UIReader reader)
        {
            sampler = reader.ReadUInt32();
            bindPoint = reader.ReadInt32();
        }
    }
}
