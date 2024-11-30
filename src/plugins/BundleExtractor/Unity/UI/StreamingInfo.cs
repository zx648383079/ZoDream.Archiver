using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class StreamingInfo
    {
        public long offset; //ulong
        public uint size;
        public string path = string.Empty;

        public StreamingInfo() {}

        public StreamingInfo(UIReader reader)
        {
            var version = reader.Version;

            if (version.Major >= 2020) //2020.1 and up
            {
                offset = reader.ReadInt64();
            }
            else
            {
                offset = reader.ReadUInt32();
            }
            size = reader.ReadUInt32();
            path = reader.ReadAlignedString();
        }
    }
}
