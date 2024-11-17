﻿using System.Collections.Generic;

namespace ZoDream.Shared.Bundle
{
    public interface IBundleEngine: IBundleLoader
    {

        public IEnumerable<IBundleChunk> EnumerateChunk(IBundleSource fileItems);

        public IBundleReader OpenRead(IBundleChunk fileItems, IBundleOptions options);
    }
}
