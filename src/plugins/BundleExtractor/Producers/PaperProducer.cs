﻿using System;
using System.Collections.Generic;
using ZoDream.Shared.Interfaces;

namespace ZoDream.BundleExtractor.Producers
{
    public class PaperProducer : IBundleProducer
    {
        // Love Nikki bf_cbc fd1c1b2f34a0d1d246be3ba9bc5af022e83375f315a0216085d3013a

        public bool TryLoad(IEnumerable<string> fileItems)
        {
            return false;
        }
    }
}
