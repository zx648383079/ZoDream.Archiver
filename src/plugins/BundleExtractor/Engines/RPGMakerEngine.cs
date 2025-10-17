using System;
using System.IO;
using System.Linq;
using ZoDream.BundleExtractor.RpgMarker;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Engines
{
    public class RPGMakerEngine(IEntryService service) : IBundleEngine, IBundleFilter
    {
        internal const string EngineName = "RPG Maker";

        public string AliasName => EngineName;
        public IBundleSplitter CreateSplitter(IBundleOptions options)
        {
            return new BundleSplitter(options is IBundleExtractOptions o ? Math.Max(o.MaxBatchCount, 1) : 100);
        }

        public IBundleSource Unpack(IBundleSource fileItems, IBundleOptions options)
        {
            if (string.IsNullOrEmpty(options.Password))
            {
                TryGetKey(fileItems, options);
            }
            return fileItems;
        }
        public IDependencyBuilder GetBuilder(IBundleOptions options)
        {
            return new DependencyBuilder(options is IBundleExtractOptions o ? o.DependencySource : string.Empty);
        }
        public IBundleHandler CreateHandler(IBundleChunk fileItems, IBundleOptions options)
        {
            return new MvScheme(fileItems, service, options);
        }

        public bool IsMatch(IFilePath filePath)
        {
            if (Path.GetExtension(filePath.Name) is ".ogg_" or ".png_" or ".m4a_" 
                or ".rpgmvp" or ".rpgmvm" or ".rpgmvo")
            {
                return false;
            }
            return true;
        }

        public bool TryLoad(IBundleSource fileItems, IBundleOptions options)
        {
            if (fileItems.GetFiles("*.rpgmv*", "*.rgss*").Any())
            {
                options.Engine = EngineName;
                TryGetKey(fileItems, options);
                return true;
            }
            return false;
        }

        private void TryGetKey(IBundleSource fileItems, IBundleOptions options)
        {
            if (options is not BundleOptions o)
            {
                return;
            }
            foreach (var item in fileItems.GetFiles("System.json"))
            {
                using var fs = fileItems.OpenRead(new FilePath(item));
                if (MvScheme.TryGetKeyFromJson(fs, out var key))
                {
                    o.Password = key;
                    return;
                }
            }
            if (string.IsNullOrEmpty(o.Password))
            {
                foreach (var item in fileItems.GetFiles("*.png_", "*.rpgmvp"))
                {
                    using var fs = fileItems.OpenRead(new FilePath(item));
                    o.Password = MvScheme.GetKey(fs);
                    return;
                }
            }
        }

        
    }
}
