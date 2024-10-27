﻿using ZoDream.Shared.Interfaces;
using System.Collections.Generic;

namespace ZoDream.BundleExtractor
{
    public partial class BundleScheme(ILogger logger) : IBundleScheme
    {
        public IBundleReader? Load(IEnumerable<string> fileItems, IArchiveOptions? options = null)
        {
            var platform = GetPlatform(fileItems);
            if (platform == null)
            {
                return null;
            }
            logger.Info(platform.GetType().Name);
            return new BundleReader(platform, options);
        }

        public IBundleReader? Load(IBundlePlatform? platform, IEnumerable<string> fileItems, IArchiveOptions? options = null)
        {
            if (platform is null)
            {
                return Load(fileItems, options);
            }
            platform.TryLoad(fileItems);
            return new BundleReader(platform, options);
        }
    }
}