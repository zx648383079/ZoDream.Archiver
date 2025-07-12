using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
using System.Threading;

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

        private readonly int _hasCode;
        private IBundleFilter? _filter;

        private readonly string[] _entryItems;
        private readonly string[][] _cacheItems;

        public uint Index { get; set; }

        /// <summary>
        /// 获取文件的数量，必须先调用 Analyze 方法
        /// </summary>
        public uint Count { get; set; }

        /// <summary>
        /// 重新计算文件的数量
        /// </summary>
        /// <returns></returns>
        public uint Analyze(CancellationToken token = default)
        {
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
                Count += (uint)items.Count;
            }
            return Count;
        }

        public uint Analyze(IBundleFilter filter, CancellationToken token = default)
        {
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
            var index = 0u;
            var begin = Index;
            for (int i = 0; i < _cacheItems.Length; i++)
            {
                if (_cacheItems[i].Length == 0)
                {
                    continue;
                }
                var end = index + (uint)_cacheItems[i].Length;
                if (end < begin)
                {
                    index = end;
                    continue;
                }
                var offset = (int)Math.Max(begin - index, 0);
                while (offset < _cacheItems[i].Length)
                {
                    var count = Math.Min(_cacheItems[i].Length - offset, maxFileCount - items.Count);
                    items.AddRange(_cacheItems[i].Skip(offset).Take(count));
                    offset += items.Count;
                    Index = index + (uint)offset;
                    yield return new BundleChunk(_entryItems[i], [.. items]);
                    items.Clear();
                }
                index = end;
            }
        }

        public IEnumerable<IBundleChunk> EnumerateChunk(IDependencyDictionary dependencies)
        {
            var index = 0u;
            var begin = Index;
            var exclude = new HashSet<string>();
            for (int i = 0; i < _cacheItems.Length; i++)
            {
                if (_cacheItems[i].Length == 0)
                {
                    continue;
                }
                var end = index + (uint)_cacheItems[i].Length;
                if (end < begin)
                {
                    index = end;
                    continue;
                }
                var offset = Math.Max(begin - index, 0);
                while (offset < _cacheItems[i].Length)
                {
                    var item = _cacheItems[i][offset ++];
                    if (exclude.Contains(item))
                    {
                        continue;
                    }
                    Index = index + offset;
                    if (!dependencies.TryGet(item, out var items))
                    {
                        yield return new BundleChunk(_entryItems[i], [item]);
                        continue;
                    }
                    if (items.Length > BundleSource.CHUNK_MAX_DEPENDENCY)
                    {
                        // 存在一些旧的文件依赖新的文件导致存在重复引用，所以干脆放弃部分导出的
                        items = [.. items.Where(i => !exclude.Contains(i))];
                    }
                    yield return new BundleChunk(_entryItems, [item, .. items]);
                    foreach (var it in items)
                    {
                        exclude.Add(item);
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
