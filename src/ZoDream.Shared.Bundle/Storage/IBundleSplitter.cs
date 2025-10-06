using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.Bundle
{
    public interface IBundleSplitter
    {
        public IEnumerable<IBundleChunk> Split(IBundleSource items);

        public bool TrySplit(IFilePath filePath, IBundleSource source, [NotNullWhen(true)] out IBundleChunk? chunk);
    }
}
