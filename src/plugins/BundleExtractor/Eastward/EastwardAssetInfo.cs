using System.Collections.Generic;

namespace ZoDream.BundleExtractor.Eastward
{
    internal readonly struct EastwardAssetInfo(string name, string filePath, string groupType, string fileType, string type, IDictionary<string, string> objectFiles)
    {
        internal readonly string FileType => fileType;
        internal readonly string AssetName => name;
        internal readonly IDictionary<string, string> ObjectFiles => objectFiles;

        internal readonly string Type => type;
    }
}
