using System;
using ZoDream.Shared.Bundle;
using UnityEngine;

namespace ZoDream.BundleExtractor
{
    public partial class BundleScheme
    {
        private IBundleEngine[] _engineItems = [];

        public string[] EngineNames => GetNames(_engineItems);


        public static string FormatEntryType(int type)
        {
            return Enum.GetName((NativeClassID)type);
        }
    }
}
