using System;
using System.Threading;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor
{
    public class BundleReader(BundleSource fileItems, 
        IBundleOptions options, BundleScheme scheme) : IBundleReader
    {

        public void ExtractTo(string folder, ArchiveExtractMode mode, CancellationToken token = default)
        {
            var engine = scheme.Get<IBundleEngine>(options);
            if (engine is null)
            {
                return;
            }
            foreach (var items in engine.EnumerateChunk(fileItems, options))
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                try
                {
                    using var chunk = engine.OpenRead(items, options);
                    chunk?.ExtractTo(folder, mode, token);
                }
                catch (Exception ex)
                {
                    scheme.Get<ILogger>().Error(ex.Message);
                }
            }
        }

        public void Dispose()
        {
        }
    }
}
