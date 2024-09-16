using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ZoDream.BundleExtractor.Platforms;
using ZoDream.Shared.Interfaces;

namespace ZoDream.BundleExtractor
{
    public class UnityBundleReader : IBundleReader
    {
        private readonly IEnumerable<string> _fileItems;
        private readonly IPlatformScheme _platform;
        private readonly IArchiveOptions? _options;

        public UnityBundleReader(IEnumerable<string> fileItems, 
            IPlatformScheme platform, 
            IArchiveOptions? options)
        {
            _fileItems = fileItems;
            _platform = platform;
            _options = options;
        }

        

        public void ExtractTo(string folder, CancellationToken token = default)
        {
            
        }

        public void Dispose()
        {
        }
    }
}
