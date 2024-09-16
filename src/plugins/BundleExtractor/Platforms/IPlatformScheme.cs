using System;
using System.Collections.Generic;

namespace ZoDream.BundleExtractor.Platforms
{
    public interface IPlatformScheme
    {
        public string Root { get; }
        public bool TryLoad(IEnumerable<string> fileItems);
    }
}
