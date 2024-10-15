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


        private readonly IBundlePlatform _platform;
        private readonly IArchiveOptions? _options;

        public void ExtractTo(string folder, ArchiveExtractMode mode, CancellationToken token = default)
        {
            foreach (var items in _platform.Engine.EnumerateChunk())
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                using var chunk = _platform.Engine.OpenRead(items);
                chunk.ExtractTo(folder, mode, token);
            }
        }

        public void Dispose()
        {
        }

        


    }
}
