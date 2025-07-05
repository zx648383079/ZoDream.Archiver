using System;
using System.Text.Json;
using System.Threading.Tasks;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.EroLabs
{
    public class LsnContext : IBundleExecutor
    {
        const string EntryUrl = "https://asset.lecisxqw.com/mgameasset/m3productionasset/Android/version.json";

        public bool CanExecute(IBundleRequest request)
        {
            return request is IFileRequest;
        }

        public async Task ExecuteAsync(IBundleRequest request, IBundleContext context)
        {
            if (request is not INetFileRequest file)
            {
                return;
            }
            if (file.Name == "version.json")
            {
                using var fs = file.OpenRead();
                var doc = JsonDocument.Parse(fs);
                if (doc is null)
                {
                    return;
                }
                if (doc.RootElement.TryGetProperty("assetBundleMetadatas", out var items))
                {
                    foreach (var item in items.EnumerateObject())
                    {
                        context.Enqueue(new NetRequest(request, new Uri(file.Source, item.Name), string.Empty));
                    }
                }
            }
        }
    }
}
