using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class HumanPoseMask
    {
        public uint word0;
        public uint word1;
        public uint word2;

        public HumanPoseMask(UIReader reader)
        {
            var version = reader.Version;

            word0 = reader.ReadUInt32();
            word1 = reader.ReadUInt32();
            if (version.GreaterThanOrEquals(5, 2)) //5.2 and up
            {
                word2 = reader.ReadUInt32();
            }
        }
    }
}
