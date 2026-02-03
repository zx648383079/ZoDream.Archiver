using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ZoDream.BundleExtractor.RSGame;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.Engines
{
    public class RSGameEngine(IEntryService service) : 
        IBundleEngine, IBundleExecutor, IResourceScheme
    {
        internal const string EngineName = "RSGame";
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
            return new ResourceReader(fileItems, this, service);
        }

        public bool TryLoad(IBundleSource fileItems, IBundleOptions options)
        {
            if (fileItems.GetFiles("*.dmxpkg").Any())
            {
                options.Engine = EngineName;
                return true;
            }
            return false;
        }
        public Task<ICommandArguments?> RecognizeAsync(IStorageFileEntry filePath, CancellationToken token = default)
        {
            return Task.FromResult<ICommandArguments?>(null);
        }
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
            if (file.Name == "game_static.js")
            {
                var content = LocationStorage.ReadText(file.OpenRead());
                var host = string.Empty;
                var path = string.Empty;
                foreach (Match item in Regex.Matches(content, @"""(.+?)"""))
                {
                    var str = item.Groups[1].Value;
                    if (string.IsNullOrWhiteSpace(str) || str.Contains("localhost") || str.Contains("test"))
                    {
                        continue;
                    }
                    if (str.Contains("://"))
                    {
                        host = str;
                        continue;
                    }
                    path = str;
                }
                context.Enqueue(new NetRequest(request, new Uri(host + path), string.Empty));
            } else if (file.Name == "hash.dat")
            {
                using var r = new StreamReader(file.OpenRead());
                while (true)
                {
                    var line = r.ReadLine();
                    if (line == null)
                    {
                        break;
                    }
                    var i = line.LastIndexOf(':');
                    if (i >= 0)
                    {
                        line = line[..(i - 1)];
                    }
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }
                    context.Enqueue(new NetRequest(request, new Uri(file.Source, line), string.Empty));
                }
            }
            else if (file.Name.EndsWith(".dmxpkg"))
            {

            }
        }

        public IBundleHandler? Open(string fileName)
        {
            if (!fileName.EndsWith(".dmxpkg"))
            {
                return null;
            }
            return new DmxPkgReader(File.OpenRead(fileName));
        }

        public IBundleHandler? Open(Stream stream, string fileName)
        {
            if (!fileName.EndsWith(".dmxpkg"))
            {
                return null;
            }
            return new DmxPkgReader(stream);
        }
    }
}
