using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
using System.Threading;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.Bundle
{
    public class BundleSource : IBundleSource
    {
        public BundleSource(IEnumerable<string> fileItems)
        {
            _entryItems = [.. fileItems.Order()];
            _hasCode = BundleStorage.ToHashCode(_entryItems);
        }

        public BundleSource(IEnumerable<string> fileItems, IEntryService service)
            : this(fileItems)
        {
            _service = service;
        }

        private readonly IEntryService? _service;
        private readonly int _hasCode;
        private IBundleFilter? _filter;
        private readonly string[] _entryItems;
        private uint _recordIndex;

        /// <summary>
        /// 获取文件的数量，必须先调用 Analyze 方法
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// 重新计算文件的数量
        /// </summary>
        /// <returns></returns>
        public int Analyze(CancellationToken token = default)
        {
            _service?.TryLoadPoint(_hasCode, out _recordIndex);
            return Count = BundleStorage.FileCount(_entryItems, token);
        }

        public int Analyze(IBundleFilter filter, CancellationToken token = default)
        {
            _filter = filter;
            return Analyze(token);
        }

        public void Breakpoint()
        {
            _service?.SavePoint(_hasCode, _recordIndex);
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
            var items = new List<string>();
            var options = new EnumerationOptions()
            {
                RecurseSubdirectories = true,
                MatchType = MatchType.Win32,
                AttributesToSkip = FileAttributes.None,
                IgnoreInaccessible = false
            };
            foreach (var item in _entryItems)
            {
                if (File.Exists(item))
                {
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
                    items.Add(it);
                    if (items.Count >= maxFileCount)
                    {
                        yield return new BundleChunk(item, [.. items]);
                        items.Clear();
                    }
                }
                if (items.Count > 0)
                {
                    yield return new BundleChunk(item, [.. items]);
                    items.Clear();
                }
            }
        }

        public IEnumerable<IBundleChunk> EnumerateChunk(IDependencyDictionary dependencies)
        {
            foreach (var item in GetFiles())
            {
                if (dependencies.TryGet(item, out var items))
                {
                    yield return new BundleChunk(dependencies.Entrance, [.. items, item]);
                }
                yield return new BundleChunk(item);
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
