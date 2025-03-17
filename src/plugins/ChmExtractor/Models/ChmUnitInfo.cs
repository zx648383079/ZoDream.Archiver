namespace ZoDream.ChmExtractor.Models
{
    internal class ChmUnitInfo
    {

        internal const string CHMU_RESET_TABLE = "::DataSpace/Storage/MSCompressed/Transform/{7FC28940-9D31-11D0-9B27-00A0C91E9C7C}/InstanceData/ResetTable";
        internal const string CHMU_CONTENT = "::DataSpace/Storage/MSCompressed/Content";
        internal const string CHMU_LZXC_CONTROLDATA = "::DataSpace/Storage/MSCompressed/ControlData";
        internal const string CHMU_SPANINFO = "::DataSpace/Storage/MSCompressed/SpanInfo";

        public long Start { get; set; }
        public long Length { get; set; }
        public int Space { get; set; }
        public int Flags { get; set; }
        public string FileName { get; set; } = string.Empty;
    }
}
