using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ZoDream.BundleExtractor.Egret;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.Engines
{
    public class EgretEngine : IBundleEngine, IBundleExecutor
    {
        internal const string EngineName = "Egret";
        public string AliasName => EngineName;

        public IBundleSplitter CreateSplitter(IBundleOptions options)
        {
            return new BundleSplitter(options is IBundleExtractOptions o ? Math.Max(o.MaxBatchCount, 1) : 100);
        }

        public IBundleSource Unpack(IBundleSource fileItems, IBundleOptions options)
        {
            return fileItems;
        }


        public IDependencyBuilder GetBuilder(IBundleOptions options)
        {
            return new DependencyBuilder(options is IBundleExtractOptions o ? o.DependencySource : string.Empty);
        }

        public IBundleHandler CreateHandler(IBundleChunk fileItems, IBundleOptions options)
        {
            return null;
        }

        public bool TryLoad(IBundleSource fileItems, IBundleOptions options)
        {
            if (fileItems.GetFiles("default.res.json").Any())
            {
                options.Engine = EngineName;
                return true;
            }
            return false;
        }

        public bool CanExecute(IBundleRequest request)
        {
            return request is IFileRequest f && f.Name.EndsWith(".json");
        }

        public async Task ExecuteAsync(IBundleRequest request, IBundleContext context)
        {
            if (request is not INetFileRequest file)
            {
                return;
            }
            using var fs = file.OpenRead();
            var content = LocationStorage.ReadText(fs);
            if (string.IsNullOrWhiteSpace(content))
            {
                return;
            }
            if (content.Contains("\"resources\""))
            {
                var data = JsonSerializer.Deserialize<ResMap>(content);
                if (data?.Resources is not null)
                {
                    foreach (var item in data.Resources)
                    {
                        context.Enqueue(new NetRequest(request, new Uri(file.Source, item.Url), string.Empty));
                        if (item.Url.EndsWith("_tex.json"))
                        {
                            context.Enqueue(new NetRequest(request, new Uri(file.Source, item.Url.Replace("_tex.json", "_ske.json")), string.Empty));
                        }
                    }
                }
            }
            else if (content.Contains("\"imagePath\""))
            {
                var data = JsonSerializer.Deserialize<JsonFile>(content);
                if (data is not null)
                {
                    context.Enqueue(new NetRequest(request, new Uri(file.Source, data.ImagePath), string.Empty));
                }
            }
            else if (content.Contains("\"frames\""))
            {
                var data = JsonSerializer.Deserialize<FrameSheetFile>(content);
                if (data is not null)
                {
                    context.Enqueue(new NetRequest(request, new Uri(file.Source, data.File), string.Empty));
                }
            }
        }
    }
}
