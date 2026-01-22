using System;
using System.Threading;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Logging;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.IFAction
{
    public class IFActionScheme(IBundleChunk fileItems, IEntryService service, IBundleOptions options) : IBundleHandler
    {
        
        public void ExtractTo(string folder, ArchiveExtractMode mode, CancellationToken token = default)
        {
            var logger = service.Get<ILogger>();
            var progress = logger.CreateSubProgress("Decrypt ...", fileItems.Count);
            foreach (var item in fileItems.Items)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                if (!item.Name.Equals("iFCon", StringComparison.OrdinalIgnoreCase))
                {
                    progress?.Add(1);
                    continue;
                }
                using var fs = fileItems.OpenRead(item);
                using var reader = new IFConReader(fs, options);
                reader.ExtractToDirectory(folder, mode, null, token);
                progress?.Add(1);
            }
        }

        public void Dispose()
        {
        }
    }
}
