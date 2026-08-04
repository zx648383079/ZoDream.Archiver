using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ZoDream.BundleExtractor.RenPy;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;

namespace ZoDream.BundleExtractor.Engines
{
    public class RenPyEngine(IEntryService service) : IBundleEngine
    {
        internal const string EngineName = "RenPy";

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
            return new ResourceReader(fileItems, new RenPyScheme(service, options), service);
        }

        public bool TryLoad(IBundleSource fileItems, IBundleOptions options)
        {
            if (fileItems.GetDirectories("renpy").Any() 
                || fileItems.GetFiles("*.rpa").Any())
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
    }
}
