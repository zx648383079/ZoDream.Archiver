using System;
using System.Collections.Generic;
using System.Linq;

namespace ZoDream.Shared.Bundle
{
    public class DependencyEntry(string fileName, long offset, IList<string> dependencies) : IDependencyEntry
    {
        public string Path { get; private set; } = fileName;

        public long Offset { get; private set; } = offset;

        public IList<string> Dependencies { get; private set; } = dependencies;


        public void Add(params string[] dependencies)
        {
            foreach (var item in dependencies)
            {
                if (Dependencies.Contains(item))
                {
                    continue;
                }
                Dependencies.Add(item);
            }
        }
    }

    public class DependencyDictionary: Dictionary<string, IDependencyEntry>, IDependencyDictionary
    {

        public string FileName { get; set; } = string.Empty;
        public string Entrance { get; set; } = string.Empty;

        public void Add(string key, params string[] dependencies)
        {
            if (TryGetValue(key, out var entry))
            {
                entry.Add(dependencies);
                return;
            }
            Add(key, new DependencyEntry(key, 0, dependencies));
        }

        public bool TryGet(string fileName, out string[] items)
        {
            return TryGet([fileName], out items);
        }

        public bool TryGet(IEnumerable<string> files, out string[] items)
        {
            return TryGet(files, false, out items);
        }

        public bool TryGet(IEnumerable<string> files, Func<string, bool>? filterFn, out string[] items)
        {
            var res = new HashSet<string>();
            var source = files.ToArray();
            var data = source;
            while (data.Length > 0)
            {
                var next = new List<string>();
                foreach (var item in data)
                {
                    if (!TryGetValue(item, out var entry))
                    {
                        continue;
                    }
                    foreach (var it in entry.Dependencies)
                    {
                        if (source.Contains(it) || res.Contains(it))
                        {
                            continue;
                        }
                        if (filterFn?.Invoke(it) != false)
                        {
                            next.Add(it);
                        }
                        res.Add(it);
                    }
                }
                data = [.. next];
            }
            items = [.. res];
            return items.Length > 0;
        }

        public bool TryGet(IEnumerable<string> files, bool isFast, out string[] items)
        {
            if (isFast)
            {
                return TryFastGet(files, out items);
            }
            #region 复杂版 // 获取全部的依赖
            var res = new HashSet<string>();
            var source = files.ToArray();
            var data = source;
            while (data.Length > 0)
            {
                var next = new List<string>();
                foreach (var item in data)
                {
                    if (!TryGetValue(item, out var entry))
                    {
                        continue;
                    }
                    foreach (var it in entry.Dependencies)
                    {
                        if (source.Contains(it) || res.Contains(it))
                        {
                            continue;
                        }
                        // 需要排除语言包的多次引用
                        next.Add(it);
                        res.Add(it);
                    }
                }
                data = [.. next];
            }
            items = [.. res];
            return items.Length > 0;
            #endregion
        }

        private bool TryFastGet(IEnumerable<string> files, out string[] items)
        {
            #region 简单版 // 只循环一次
            var source = files.ToArray();
            var res = new HashSet<string>();
            foreach (var item in source)
            {
                if (!TryGetValue(item, out var entry))
                {
                    continue;
                }
                foreach (var it in entry.Dependencies)
                {
                    if (source.Contains(it))
                    {
                        continue;
                    }
                    res.Add(it);
                }
            }
            items = [.. res];
            return items.Length > 0;
            #endregion
        }
    }
}
