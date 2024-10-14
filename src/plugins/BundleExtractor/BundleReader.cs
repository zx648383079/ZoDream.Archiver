using System.Threading;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor
{
    public class BundleReader : IBundleReader
    {
 

        public BundleReader(
            IBundlePlatform platform, 
            IArchiveOptions? options)
        {
            _platform = platform;
            _options = options;
        }

        public BundleReader(BundleScheme scheme, IBundlePlatform platform,
            IArchiveOptions? options): this(platform, options)
        {
            _scheme = scheme;
        }

        private readonly IBundlePlatform _platform;
        private readonly IArchiveOptions? _options;
        private readonly BundleScheme? _scheme;

        public void ExtractTo(string folder, ArchiveExtractMode mode, CancellationToken token = default)
        {
            foreach (var items in _platform.Producer.EnumerateChunk())
            {
                using var chunk = _platform.Engine.OpenRead(items);
                chunk.ExtractTo(folder, mode, token);
            }
        }

        public void Dispose()
        {
        }

        


    }
}
