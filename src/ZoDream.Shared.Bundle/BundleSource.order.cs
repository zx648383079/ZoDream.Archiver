using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
using System.Threading;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.Bundle
{
    public class BundleOrderSource : IBundleSource
    {

        public BundleOrderSource(IEnumerable<string> fileItems)
        {
            _entryItems = [.. fileItems.Order()];
            _cacheItems = new string[_entryItems.Length][];
            _hasCode = BundleStorage.ToHashCode(_entryItems);
        }

        public BundleOrderSource(IEnumerable<string> fileItems, IEntryService service)
            : this(fileItems)
        {
            _service = service;
        }

        private readonly IEntryService? _service;
        private readonly int _hasCode;
        private IBundleFilter? _filter;
        private uint _recordIndex;

        private readonly string[] _entryItems;
        private readonly string[][] _cacheItems;

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
            var options = new EnumerationOptions()
            {
                RecurseSubdirectories = true,
                MatchType = MatchType.Win32,
                AttributesToSkip = FileAttributes.None,
                IgnoreInaccessible = false
            };
            for (var i = 0; i < _entryItems.Length; i ++)
            {
                var item = _entryItems[i];
                if (File.Exists(item))
                {
                    Count++;
                    _cacheItems[i] = [item];
                    continue;
                }
                var items = new List<string>();
                var res = new FileSystemEnumerable<string>(item, delegate (ref FileSystemEntry entry)
                {
                    return entry.ToSpecifiedFullPath();
                }, options)
                {
                    ShouldIncludePredicate = delegate (ref FileSystemEntry entry)
                    {
                        return entry.IsDirectory;
                    }
                };
                foreach (var it in res)
                {
                    if (_filter?.IsExclude(it) == true)
                    {
                        continue;
                    }
                    items.Add(it);
                }
                _cacheItems[i] = [..items.Order()];
                Count += items.Count;
            }
            return Count;
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
            var index = 0;
            var begin = 0;
            for (int i = 0; i < _cacheItems.Length; i++)
            {
                if (_cacheItems[i].Length == 0)
                {
                    continue;
                }
                var end = i + _cacheItems[i].Length;
                if (end < begin)
                {
                    index = end;
                    continue;
                }
                var offset = Math.Max(begin - index, 0);
                var count = _cacheItems[i].Length - offset;
                yield return new BundleChunk(_entryItems[i], _cacheItems[i].Skip(offset).Take(count));
                index = end;
            }
        }

        public IEnumerable<IBundleChunk> EnumerateChunk(int maxFileCount)
        {
            var items = new List<string>();
            var index = 0;
            var begin = 0;
            for (int i = 0; i < _cacheItems.Length; i++)
            {
                if (_cacheItems[i].Length == 0)
                {
                    continue;
                }
                var end = i + _cacheItems[i].Length;
                if (end < begin)
                {
                    index = end;
                    continue;
                }
                var offset = Math.Max(begin - index, 0);
                while (offset < _cacheItems[i].Length)
                {
                    var count = Math.Min(_cacheItems[i].Length - offset, maxFileCount - items.Count);
                    items.AddRange(_cacheItems[i].Skip(offset).Take(count));
                    yield return new BundleChunk(_entryItems[i], [.. items]);
                    offset += items.Count;
                    items.Clear();
                }
                index = end;
            }
        }

        public IEnumerable<IBundleChunk> EnumerateChunk(IDependencyDictionary dependencies)
        {
            var index = 0;
            var begin = 0;
            var excludeItems = new HashSet<int>();
            for (int i = 0; i < _cacheItems.Length; i++)
            {
                if (_cacheItems[i].Length == 0)
                {
                    continue;
                }
                var end = i + _cacheItems[i].Length;
                if (end < begin)
                {
                    index = end;
                    continue;
                }
                var offset = Math.Max(begin - index, 0);
                while (offset < _cacheItems[i].Length)
                {
                    var item = _cacheItems[i][offset ++];
                    if (excludeItems.Contains(item.GetHashCode()))
                    {
                        continue;
                    }
                    if (!dependencies.TryGet(item, out var items))
                    {
                        yield return new BundleChunk(_entryItems[i], [item]);
                        continue;
                    }
                    yield return new BundleChunk(_entryItems[i], [item, ..items]);
                    foreach (var it in items)
                    {
                        excludeItems.Add(item.GetHashCode());
                    }
                }
                index = end;
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
}
