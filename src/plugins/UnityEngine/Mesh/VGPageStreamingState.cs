using ZoDream.Shared.Bundle;

namespace UnityEngine
{
    public class VGPageStreamingState
    {
        public uint BulkOffset { get; set; }
        public uint BulkSize { get; set; }
        public uint PageSize { get; set; }
        public uint DependenciesStart { get; set; }
        public uint DependenciesNum { get; set; }
        public uint Flags { get; set; }

    }
}
