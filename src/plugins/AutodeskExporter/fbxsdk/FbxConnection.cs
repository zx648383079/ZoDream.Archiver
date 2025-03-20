namespace ZoDream.AutodeskExporter
{
    internal class FbxConnection
    {
        internal enum EType: uint
        {
            eNone = 0,
            // System or user
            eSystem = 1 << 0,
            eUser = 1 << 1,
            eSystemOrUser = eUser | eSystem,
            // Type of Link
            eReference = 1 << 2,
            eContains = 1 << 3,
            eData = 1 << 4,
            eLinkType = eReference | eContains | eData,
            eDefault = eUser | eReference,
            eUnidirectional = 1 << 7
        };
    }
}
