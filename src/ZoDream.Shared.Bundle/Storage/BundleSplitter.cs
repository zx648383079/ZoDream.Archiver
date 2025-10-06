using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.Shared.Bundle
{
    public class BundleSplitter(int maxFileCount) : IBundleSplitter
    {

        private readonly IList<IFilePath> _chunkItems = [];

        public IEnumerable<IBundleChunk> Split(IBundleSource items)
        {
            foreach (var item in items.GetFiles())
            {
                if (TrySplit(FilePath.Parse(item), items, out var chunk))
                {
                    yield return chunk;
                }
            }
        }

        public bool TrySplit(IFilePath filePath, IBundleSource source, [NotNullWhen(true)] out IBundleChunk? chunk)
        {
            _chunkItems.Add(filePath);
            if (_chunkItems.Count < maxFileCount)
            {
                chunk = null;
                return false;
            }
            chunk = new BundleChunk(source, _chunkItems.ToArray(), []);
            _chunkItems.Clear();
            return true;
        }
    }
}
