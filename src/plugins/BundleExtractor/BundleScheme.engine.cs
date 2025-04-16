using System;
using ZoDream.BundleExtractor.Unity.UI;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor
{
    public partial class BundleScheme
    {
        private IBundleEngine[] _engineItems = [];

        public string[] EngineNames => GetNames(_engineItems);


        public static string FormatEntryType(int type)
        {
            return Enum.GetName((ElementIDType)type);
        }
    }
}
