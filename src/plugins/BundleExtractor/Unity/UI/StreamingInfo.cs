using ZoDream.BundleExtractor.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class StreamingInfo
    {
        public long offset; //ulong
        public uint size;
        public string path = string.Empty;

        public StreamingInfo() {}

        public StreamingInfo(IBundleBinaryReader reader)
        {
            var version = reader.Get<UnityVersion>();

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
