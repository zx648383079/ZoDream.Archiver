using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.Shared.Bundle
{
    public partial class DependencyBuilder : IDependencyBuilder
    {
        /// <summary>
        /// 不做任何处理
        /// </summary>
        public DependencyBuilder()
        {
            
        }
        public DependencyBuilder(string fullPath)
        {
            if (!string.IsNullOrWhiteSpace(fullPath))
            {
                _writer = new BinaryWriter(File.Create(fullPath), Encoding.UTF8);
            }
        }

        public DependencyBuilder(Stream output)
        {
            _writer = new BinaryWriter(output, Encoding.UTF8);
        }

        private readonly BinaryWriter? _writer;

        private readonly Dictionary<string, DependencyEntry> _items = [];

        public void AddDependency(string fileName, string dependencyFileName)
        {
            if (_writer is null)
            {
                return;
            }
            AddDependency(FilePath.Parse(fileName), FilePath.Parse(dependencyFileName));
        }

        public void AddDependency(IFilePath source, IFilePath dependencyPath)
        {
            if (_writer is null)
            {
                return;
            }
            if (source is IEntryPath e)
            {
                AddVerifyEntry(e.FilePath, e.EntryPath);
            }
            var fileName = FilePath.GetFilePath(source);
            if (!_items.TryGetValue(fileName, out var item))
            {
                _items.Add(fileName, item = new());
            }
            if (dependencyPath is IEntryPath d)
            {
                AddVerifyEntry(d.FilePath, d.EntryPath);
                item.AddLink(d.EntryPath);
            }
            item.AddLink(FilePath.GetFilePath(dependencyPath));
        }

        public void AddDependencyEntry(string fileName, long dependencyEntryId)
        {
            if (_writer is null)
            {
                return;
            }
            AddDependencyEntry(FilePath.Parse(fileName), dependencyEntryId);
        }

        public void AddDependencyEntry(IFilePath source, long dependencyEntryId)
        {
            if (_writer is null)
            {
                return;
            }
            if (source is IEntryPath e)
            {
                AddVerifyEntry(e.FilePath, e.EntryPath);
            }
            var fileName = FilePath.GetFilePath(source);
            if (!_items.TryGetValue(fileName, out var item))
            {
                _items.Add(fileName, item = new());
            }
            item.AddLink(dependencyEntryId);
        }

        public void AddDependencyEntry(string fileName, long entryId, long dependencyEntryId)
        {
            if (_writer is null)
            {
                return;
            }
            AddDependencyEntry(FilePath.Parse(fileName), entryId, dependencyEntryId);
        }

        public void AddDependencyEntry(IFilePath source, long entryId, long dependencyEntryId)
        {
            if (_writer is null)
            {
                return;
            }
            if (source is IEntryPath e)
            {
                AddVerifyEntry(e.FilePath, e.EntryPath);
            }
            var fileName = FilePath.GetFilePath(source);
            if (!_items.TryGetValue(fileName, out var item))
            {
                _items.Add(fileName, item = new());
            }
            item.Add(entryId);
            item.AddLink(dependencyEntryId);
        }

        public void AddDependencyEntry(string fileName, long entryId, string dependencyEntryName)
        {
            if (_writer is null)
            {
                return;
            }
            AddDependencyEntry(FilePath.Parse(fileName), entryId, dependencyEntryName);
        }

        public void AddDependencyEntry(IFilePath source, long entryId, string dependencyEntryName)
        {
            if (_writer is null)
            {
                return;
            }
            if (source is IEntryPath e)
            {
                AddVerifyEntry(e.FilePath, e.EntryPath);
            }
            var fileName = FilePath.GetFilePath(source);
            if (!_items.TryGetValue(fileName, out var item))
            {
                _items.Add(fileName, item = new());
            }
            item.Add(entryId);
            item.AddLink(dependencyEntryName);
        }
        public void AddDependencyEntry(string fileName, string entryName, string dependencyEntryName)
        {
            if (_writer is null)
            {
                return;
            }
            AddDependencyEntry(FilePath.Parse(fileName), entryName, dependencyEntryName);
        }

        public void AddDependencyEntry(IFilePath source, string entryName, string dependencyEntryName)
        {
            if (_writer is null)
            {
                return;
            }
            if (source is IEntryPath e)
            {
                AddVerifyEntry(e.FilePath, e.EntryPath);
            }
            var fileName = FilePath.GetFilePath(source);
            if (!_items.TryGetValue(fileName, out var item))
            {
                _items.Add(fileName, item = new());
            }
            item.Add(entryName);
            item.AddLink(dependencyEntryName);
        }

        public void AddDependencyEntry(string fileName, string dependencyEntryName)
        {
            if (_writer is null)
            {
                return;
            }
            AddDependencyEntry(FilePath.Parse(fileName), dependencyEntryName);
        }

        public void AddDependencyEntry(IFilePath source, string dependencyEntryName)
        {
            if (_writer is null)
            {
                return;
            }
            if (source is IEntryPath e)
            {
                AddVerifyEntry(e.FilePath, e.EntryPath);
            }
            var fileName = FilePath.GetFilePath(source);
            if (!_items.TryGetValue(fileName, out var item))
            {
                _items.Add(fileName, item = new());
            }
            item.AddLink(dependencyEntryName);
        }

        public void AddEntry(string fileName, long entryId)
        {
            if (_writer is null)
            {
                return;
            }
            AddEntry(FilePath.Parse(fileName), entryId);
        }

        public void AddEntry(IFilePath source, long entryId)
        {
            if (_writer is null)
            {
                return;
            }
            if (source is IEntryPath e)
            {
                AddVerifyEntry(e.FilePath, e.EntryPath);
            }
            var fileName = FilePath.GetFilePath(source);
            if (!_items.TryGetValue(fileName, out var item))
            {
                _items.Add(fileName, item = new());
            }
            item.Add(entryId);
        }

        public void AddEntry(string fileName, long entryId, string entryName, int entryType)
        {
            if (_writer is null)
            {
                return;
            }
            AddEntry(FilePath.Parse(fileName), entryId, entryName, entryType);
        }

        public void AddEntry(IFilePath source, long entryId, string entryName, int entryType)
        {
            if (_writer is null)
            {
                return;
            }
            if (source is IEntryPath e)
            {
                AddVerifyEntry(e.FilePath, e.EntryPath);
            }
            var fileName = FilePath.GetFilePath(source);
            if (!_items.TryGetValue(fileName, out var item))
            {
                _items.Add(fileName, item = new());
            }
            item.Add(entryId, entryName, entryType);
        }
        /// <summary>
        /// 判断是否包含文件路径
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public bool Contains(IFilePath source)
        {
            return _items.ContainsKey(FilePath.GetFilePath(source));
        }

        public void AddEntry(string fileName, string entryName)
        {
            if (_writer is null)
            {
                return;
            }
            AddEntry(FilePath.Parse(fileName), entryName);
        }
        public void AddEntry(IFilePath source, string entryName)
        {
            if (_writer is null)
            {
                return;
            }
            if (source is IEntryPath e)
            {
                AddVerifyEntry(e.FilePath, e.EntryPath);
            }
            var fileName = FilePath.GetFilePath(source);
            if (!_items.TryGetValue(fileName, out var item))
            {
                _items.Add(fileName, item = new());
            }
            item.Add(entryName);
        }

        public void AddEntry(IFilePath source)
        {
            if (source is IEntryPath e)
            {
                AddVerifyEntry(e.FilePath, e.EntryPath);
            }
        }


        private void AddVerifyEntry(string fileName, string entryName)
        {
            if (!_items.TryGetValue(fileName, out var item))
            {
                _items.Add(fileName, item = new());
            }
            item.Add(entryName);
        }



        public void Load(Stream input)
        {
            var reader = new BinaryReader(input);
            while (input.Position < input.Length)
            {
                var fileName = reader.ReadString();
                var item = new DependencyEntry();
                item.Read(reader);
                _items.TryAdd(fileName, item);
            }
        }
        /// <summary>
        /// 从日志文件中提取缺失的依赖
        ///     日志规则: Need<{原文件路径}>: 依赖文件[依赖ID]
        /// </summary>
        /// <param name="input"></param>
        public void Combine(Stream input)
        {
            using var reader = new StreamReader(input);
            while (true)
            {
                var line = reader.ReadLine();
                if (line == null)
                {
                    break;
                }
                var match = LogDependencyRegex().Match(line);
                if (!match.Success)
                {
                    continue;
                }
                var sourcePath = FilePath.Parse(match.Groups[1].Value);
                if (!Contains(sourcePath))
                {
                    continue;
                }
                var dependency = match.Groups[2].Value.Trim();
                if (dependency.StartsWith('#'))
                {
                    AddDependencyEntry(sourcePath, dependency[1..]);
                    continue;
                }
                AddDependency(sourcePath, FilePath.Parse(dependency));
            }
        }

        public void Flush()
        {
            if (_writer is null)
            {
                return;
            }
            foreach (var item in _items)
            {
                _writer.Write(item.Key);
                item.Value.Write(_writer);
            }
            _items.Clear();
            _writer.Flush();
        }

        public void Dispose()
        {
            _writer?.Dispose();
        }
        public IDependencyDictionary ToDictionary()
        {
            return ToDictionary(StringComparison.Ordinal);
        }
        public IDependencyDictionary ToDictionary(StringComparison comparisonType)
        {
            var res = new DependencyDictionary();
            foreach (var item in _items)
            {
                res.Add(item.Key, GetDependency(item.Key, 
                    item.Value.LinkedItems, 
                    item.Value.LinkedPartItems, comparisonType));
            }
            return res;
        }

        /// <summary>
        /// 获取双向依赖的文件，可能存在新旧多个文件存在相同的内容
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sourceBag"></param>
        /// <param name="comparisonType"></param>
        /// <returns></returns>
        private string[] GetDependency(string source, DependencyEntry sourceBag, StringComparison comparisonType)
        {
            var res = new HashSet<string>();
            foreach (var target in _items)
            {
                if (source == target.Key || res.Contains(target.Key))
                {
                    continue;
                }
                if (sourceBag.Contains(target.Value, comparisonType) 
                    || target.Value.Contains(sourceBag, comparisonType))
                {
                    res.Add(target.Key);
                }
            }
            return res.ToArray();
        }
        /// <summary>
        /// 根据依赖部分获取依赖的文件，只取第一个包含的依赖文件
        /// </summary>
        /// <param name="source"></param>
        /// <param name="linkedItems"></param>
        /// <param name="linkedPartItems"></param>
        /// <param name="comparisonType"></param>
        /// <returns></returns>
        private string[] GetDependency(string source, 
            IEnumerable<long> linkedItems,
            IEnumerable<string> linkedPartItems, StringComparison comparisonType)
        {
            var res = new HashSet<string>();
            foreach (var item in linkedItems)
            {
                foreach (var target in _items)
                {
                    if (source == target.Key || res.Contains(target.Key))
                    {
                        continue;
                    }
                    if (target.Value.Contains(item))
                    {
                        res.Add(target.Key);
                    }
                }
            }
            foreach (var item in linkedPartItems)
            {
                foreach (var target in _items)
                {
                    if (source == target.Key || res.Contains(target.Key))
                    {
                        continue;
                    }
                    if (target.Value.Contains(item, comparisonType))
                    {
                        res.Add(target.Key);
                    }
                }
            }
            return [..res];
        }
        /// <summary>
        /// 只读取现有的，无法进行写入
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static DependencyBuilder Load(string fileName)
        {
            using var fs = File.OpenRead(fileName);
            var builder = new DependencyBuilder();
            builder.Load(fs);
            return builder;
        }

        private struct DependencyIdEntry(long id, string name, int type)
        {
            public long Id = id;
            public int Type = type;
            public string Name = name;
        }

        private class DependencyEntry
        {
            private readonly List<DependencyIdEntry> _children = [];
            private readonly HashSet<string> _partItems = [];
            private readonly HashSet<long> _linked = [];
            private readonly HashSet<string> _linkedPart = [];

            public IEnumerable<long> LinkedItems => _linked;
            public IEnumerable<string> LinkedPartItems => _linkedPart;

            public bool Contains(DependencyEntry target)
            {
                return Contains(target, StringComparison.Ordinal);
            }
            public bool Contains(DependencyEntry target, StringComparison comparisonType)
            {
                if (this == target)
                {
                    return false;
                }
                foreach (var item in target._linkedPart)
                {
                    if (Contains(item, comparisonType))
                    {
                        return true;
                    }
                }
                foreach (var item in target._linked)
                {
                    if (Contains(item))
                    {
                        return true;
                    }
                }
                return false;
            }



            public bool Contains(string child)
            {
                return _partItems.Contains(child);
            }

            public bool Contains(string child, StringComparison comparisonType)
            {
                if (comparisonType == StringComparison.Ordinal)
                {
                    return Contains(child);
                }
                foreach (var item in _partItems)
                {
                    if (item.Equals(child, comparisonType))
                    {
                        return true;
                    }
                }
                return false;
            }

            public bool Contains(long child)
            {
                return _children.Where(i => i.Id == child).Any();
            }

            public void Add(string child)
            {
                if (string.IsNullOrWhiteSpace(child))
                {
                    return;
                }
                _partItems.Add(child);
            }

            public void Add(long child)
            {
                if (child == 0)
                {
                    return;
                }
                _children.Add(new(child, string.Empty, 0));
            }

            public void Add(long child, string name, int type)
            {
                if (child == 0 && string.IsNullOrWhiteSpace(name))
                {
                    return;
                }
                _children.Add(new(child, name, type));
            }

            public void AddLink(string link)
            {
                if (string.IsNullOrWhiteSpace(link))
                {
                    return;
                }
                if (Contains(link))
                {
                    return;
                }
                _linkedPart.Add(link);
            }

            public void AddLink(long link)
            {
                if (link == 0)
                {
                    return;
                }
                if (Contains(link))
                {
                    return;
                }
                _linked.Add(link);
            }

            public void Write(BinaryWriter writer)
            {
                writer.Write(_partItems.Count);
                foreach (var item in _partItems)
                {
                    writer.Write(item);
                }
                writer.Write(_children.Count);
                foreach (var item in _children)
                {
                    writer.Write(item.Id);
                    writer.Write(item.Type);
                    writer.Write(item.Name ?? string.Empty);
                }
                writer.Write(_linked.Count);
                foreach (var item in _linked)
                {
                    writer.Write(item);
                }
                writer.Write(_linkedPart.Count);
                foreach (var item in _linkedPart)
                {
                    writer.Write(item);
                }
            }

            public void Read(BinaryReader reader)
            {
                var count = reader.ReadInt32();
                for (var i = 0; i < count; i++)
                {
                    Add(reader.ReadString());
                }
                count = reader.ReadInt32();
                for (var i = 0; i < count; i++)
                {
                    var id = reader.ReadInt64();
                    var type = reader.ReadInt32();
                    Add(id, reader.ReadString(), type);
                }
                count = reader.ReadInt32();
                for (var i = 0; i < count; i++)
                {
                    AddLink(reader.ReadInt64());
                }
                count = reader.ReadInt32();
                for (var i = 0; i < count; i++)
                {
                    AddLink(reader.ReadString());
                }
            }
        }

        [GeneratedRegex(@"Need\<(\S+?)\>:\s(\S+?)\[\d+\]")]
        private static partial Regex LogDependencyRegex();
    }
}
