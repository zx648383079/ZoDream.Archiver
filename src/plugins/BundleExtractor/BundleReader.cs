using System.Threading;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor
{
    public class BundleReader(BundleSource fileItems, IBundleOptions options, ILogger logger) : IBundleReader
    {

        public void ExtractTo(string folder, ArchiveExtractMode mode, CancellationToken token = default)
        {
            var engine = BundleScheme.CreateEngine(options);
            if (engine is null)
            {
                return;
            }
            foreach (var items in engine.EnumerateChunk(fileItems))
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                using var chunk = engine.OpenRead(items, options);
                chunk?.ExtractTo(folder, mode, token);
            }
        }

        public void Dispose()
        {
        }
    }
}
