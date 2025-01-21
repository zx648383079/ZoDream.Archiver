using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class VGPageStreamingState
    {
        public uint BulkOffset { get; set; }
        public uint BulkSize { get; set; }
        public uint PageSize { get; set; }
        public uint DependenciesStart { get; set; }
        public uint DependenciesNum { get; set; }
        public uint Flags { get; set; }

        public VGPageStreamingState(IBundleBinaryReader reader)
        {
            BulkOffset = reader.ReadUInt32();
            BulkSize = reader.ReadUInt32();
            PageSize = reader.ReadUInt32();
            DependenciesStart = reader.ReadUInt32();
            DependenciesNum = reader.ReadUInt32();
            Flags = reader.ReadUInt32();
        }
    }
}
