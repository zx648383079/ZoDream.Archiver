namespace UnityEngine
{
    public struct VGPageStreamingState
    {
        public uint BulkOffset;
        public uint BulkSize;
        public uint PageSize;
        public uint DependenciesStart;
        public uint DependenciesNum;
        public uint Flags;

    }
}
