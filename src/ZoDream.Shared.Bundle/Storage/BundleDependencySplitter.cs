using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.Shared.Bundle.Storage
{
    public class BundleDependencySplitter(IDependencyDictionary dependencies, int maxFileCount = 5) : IBundleSplitter
    {
        private readonly IList<string> _chunkItems = [];
        public IEnumerable<IBundleChunk> Split(IBundleSource items)
        {
            IBundleChunk? chunk;
            foreach (var item in items.GetFiles())
            {
                if (TrySplit(FilePath.Parse(item), items, out chunk))
                {
                    yield return chunk;
                }
            }
            if (TryFinish(items, out chunk))
            {
                yield return chunk;
            }
        }

        public bool TrySplit(IFilePath filePath, IBundleSource source, [NotNullWhen(true)] out IBundleChunk? chunk)
        {
            _chunkItems.Add(filePath.FullPath);
            if (_chunkItems.Count < maxFileCount)
            {
                chunk = null;
                return false;
            }
            chunk = GetChunk(source);
            return true;
        }

        public bool TryFinish(IBundleSource source, [NotNullWhen(true)] out IBundleChunk? chunk)
        {
            if (_chunkItems.Count == 0)
            {
                chunk = null;
                return false;
            }
            chunk = GetChunk(source);
            return true;
        }

        private IBundleChunk GetChunk(IBundleSource source)
        {
            var items = _chunkItems.ToArray();
            _chunkItems.Clear();
            if (dependencies.TryGet(items, out var depends))
            {
                return new BundleChunk(source, items, depends);
            }
            else
            {
                return new BundleChunk(source, items, []);
            }
        }
    }
}
