using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ZoDream.BundleExtractor.Cocos;
using ZoDream.BundleExtractor.Producers;
using ZoDream.Shared.Interfaces;

namespace ZoDream.BundleExtractor.Engines
{
    public class CocosEngine : IBundleEngine, IOfPlatform
    {
        public CocosEngine()
        {
        }

        public CocosEngine(IBundlePlatform platform)
        {
            _platform = platform;
        }

        private IBundlePlatform? _platform;
        private IEnumerable<string>? _fileItems;

        public IBundlePlatform Platform 
        {
            set { _platform = value; }
        }

        public IEnumerable<IBundleChunk> EnumerateChunk()
        {
            if (_fileItems is null)
            {
                return [];
            }
            return _fileItems.Select(i => new BundleChunk(i));
        }

        public IBundleReader OpenRead(IBundleChunk fileItems)
        {
            if (_platform?.Producer is PaperProducer)
            {
                return new BlowfishReader(fileItems, "fd1c1b2f34a0d1d246be3ba9bc5af022e83375f315a0216085d3013a");
            }
            throw new NotImplementedException();
        }

        public bool TryLoad(IEnumerable<string> fileItems)
        {
            _fileItems = fileItems;
            foreach (var item in fileItems)
            {
                if (File.Exists(item))
                {
                    continue;
                }
                if (Path.GetFileName(item).StartsWith("cocos"))
                {
                    return true;
                }
                if (Directory.GetDirectories(item, "cocos", 
                    SearchOption.AllDirectories).Length > 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
