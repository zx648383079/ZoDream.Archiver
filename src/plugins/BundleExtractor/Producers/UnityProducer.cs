using BundleExtractor.Models;
using System;
using System.Collections.Generic;
using ZoDream.Shared.Interfaces;

namespace ZoDream.BundleExtractor.Producers
{
    public class UnityProducer : IBundleProducer
    {
        public IList<DependencyDictionary> DependencyItems { get; private set; } = [];

        public IEnumerable<IEnumerable<string>> EnumerateChunk()
        {
            throw new NotImplementedException();
        }

        public bool TryLoad(IEnumerable<string> fileItems)
        {
            throw new NotImplementedException();
        }
    }
}
