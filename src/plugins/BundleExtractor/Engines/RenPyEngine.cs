﻿using System.Collections.Generic;
using System.Linq;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Engines
{
    public class RenPyEngine : IBundleEngine
    {
        internal const string EngineName = "RenPy";

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
            if (fileItems.GetDirectories("renpy").Any())
            {
                options.Engine = EngineName;
                return true;
            }
            return false;
        }
    }
}
