﻿using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Platforms
{
    public class IosPlatformScheme : IBundlePlatform
    {
        internal const string PlatformName = "IOS";

        public string AliasName => PlatformName;

        public bool TryLoad(IBundleSource fileItems, IBundleOptions options)
        {
            return false;
        }
    }
}
