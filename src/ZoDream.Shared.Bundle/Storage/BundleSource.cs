using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
using System.Threading;

namespace ZoDream.Shared.Bundle
{
    public class BundleSource : IBundleSource
    {
        /// <summary>
        /// 限制一下单次依赖的数量，避免一些不必要的重复依赖
        /// </summary>
        internal const int CHUNK_MAX_DEPENDENCY = 20;
        public BundleSource(IEnumerable<string> fileItems)
        {
            _entryItems = [.. fileItems.Order()];
            _hasCode = BundleStorage.ToHashCode(_entryItems);
        }

        private readonly int _hasCode;
        private IBundleFilter? _filter;
        private readonly string[] _entryItems;

        public uint Index { get; set; }

        /// <summary>
        /// 获取文件的数量，必须先调用 Analyze 方法
        /// </summary>
        public uint Count { get; private set; }

        /// <summary>
        /// 重新计算文件的数量
        /// </summary>
        /// <returns></returns>
        public uint Analyze(CancellationToken token = default)
        {
            _filter?.Reset();
            return Count = BundleStorage.FileCount(_entryItems, token);
        }

        public uint Analyze(IBundleFilter filter, CancellationToken token = default)
        {
            _filter?.Reset();
            _filter = filter;
            return Analyze(token);
        }

        public IEnumerable<string> GetFiles(params string[] searchPatternItems)
        {
            return BundleStorage.Glob(_entryItems, searchPatternItems, SearchTarget.Files);
        }

        public IEnumerable<string> GetDirectories(params string[] searchPatternItems)
        {
            return BundleStorage.Glob(_entryItems, searchPatternItems, SearchTarget.Directories);
        }

        public IEnumerable<string> Glob(params string[] searchPatternItems)
        {
            return BundleStorage.Glob(_entryItems, searchPatternItems, SearchTarget.Both);
        }

        public IEnumerable<IBundleChunk> EnumerateChunk()
        {
            return _entryItems.Select(i => new BundleChunk(i));
        }

        public IEnumerable<IBundleChunk> EnumerateChunk(int maxFileCount)
        {
            return EnumerateChunk(maxFileCount, null);
        }

        private IEnumerable<IBundleChunk> EnumerateChunk(int maxFileCount, HashSet<string>? excludeItems)
        {
            var items = new List<string>();
            var options = new EnumerationOptions()
            {
                RecurseSubdirectories = true,
                MatchType = MatchType.Win32,
                AttributesToSkip = FileAttributes.None,
                IgnoreInaccessible = false
            };
            var index = 0u;
            var begin = Index;
            foreach (var item in _entryItems)
            {
                if (File.Exists(item))
                {
                    if (index++ < begin)
                    {
                        continue;
                    }
                    if (excludeItems?.Contains(item) == true)
                    {
                        continue;
                    }
                    Index = index;
                    yield return new BundleChunk(item);
                    continue;
                }
                var res = new FileSystemEnumerable<string>(item, delegate (ref FileSystemEntry entry)
                {
                    return entry.ToSpecifiedFullPath();
                }, options)
                {
                    ShouldIncludePredicate = delegate (ref FileSystemEntry entry)
                    {
                        return !entry.IsDirectory;
                    }
                };
                foreach (var it in res)
                {
                    if (_filter?.IsExclude(it) == true)
                    {
                        continue;
                    }
                    if (index++ < begin)
                    {
                        continue;
                    }
                    if (excludeItems?.Contains(it) == true)
                    {
                        continue;
                    }
                    items.Add(it);
                    if (items.Count >= maxFileCount)
                    {
                        Index = index;
                        yield return new BundleChunk(item, [.. items]);
                        items.Clear();
                    }
                }
                if (items.Count > 0)
                {
                    Index = index;
                    yield return new BundleChunk(item, [.. items]);
                    items.Clear();
                }
            }
        }
        public IEnumerable<IBundleChunk> EnumerateChunk(IDependencyDictionary dependencies)
        {
            var maxCount = 5;
            foreach (var item in EnumerateChunk(maxCount))
            {
                if (!dependencies.TryGet(item, out var items))
                {
                    yield return item;
                    continue;
                }
                yield return new BundleChunk(_entryItems, [.. item, ..items],
                   // 方便保存载入进度
                   maxCount);
            }
        }

        public IEnumerator<string> GetEnumerator()
        {
            foreach (var item in _entryItems)
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _entryItems.GetEnumerator();
        }

        public override int GetHashCode()
        {
            return _hasCode;
        }


    }

    public enum SearchTarget
    {
        Files = 1,
        Directories,
        Both
    }
}
