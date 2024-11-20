﻿using System.Collections.Generic;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Engines
{
    public class UnrealEngine : IBundleEngine
    {
        internal const string EngineName = "Unreal";

        public string AliasName => EngineName;
        public IEnumerable<IBundleChunk> EnumerateChunk(IBundleSource fileItems, IBundleOptions options)
        {
            foreach (var item in fileItems)
            {
                yield return new BundleChunk(item);
            }
        }

        public IBundleReader OpenRead(IBundleChunk fileItems, IBundleOptions options)
        {
            return null;
        }

        public bool TryLoad(IBundleSource fileItems, IBundleOptions options)
        {
            return false;
        }
    }
}