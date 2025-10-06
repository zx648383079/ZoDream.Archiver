using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Logging;
using ZoDream.Shared.Models;

namespace ZoDream.Shared.Bundle
{
    /// <summary>
    /// 一单个文件处理一批文件
    /// </summary>
    /// <param name="fileItems"></param>
    /// <param name="scheme"></param>
    public class ResourceReader(
        IBundleChunk fileItems, 
        IResourceScheme scheme,
        IEntryService service) : IBundleHandler
    {
        

        public void ExtractTo(string folder, ArchiveExtractMode mode, CancellationToken token = default)
        {
            var logger = service.Get<ILogger>();
            var progress = logger?.CreateSubProgress("Extract file...", 
                fileItems is IBundleChunk c ? c.Count : fileItems.Count);
            var i = 0;
            foreach (var item in fileItems.Items)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                using var reader = scheme.Open(item.FullPath);
                reader?.ExtractTo(folder, mode, token);
                if (progress is not null)
                {
                    progress.Value = i++;
                }
            }
        }

        public void Dispose()
        {

        }

    }
}
