﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.Shared.Interfaces;

namespace ZoDream.BundleExtractor.Producers
{
    public class MiHoYoProducer : IBundleProducer
    {
        public IEnumerable<IBundleChunk> EnumerateChunk()
        {
            throw new NotImplementedException();
        }

        public bool TryLoad(IEnumerable<string> fileItems)
        {
            throw new NotImplementedException();
        }
    }
}
