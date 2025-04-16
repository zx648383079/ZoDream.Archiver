using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ZoDream.Shared.Bundle
{
    public class DependencyBuilder : IDependencyBuilder
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
            fileName = BundleStorage.Separate(fileName, out var entryName);
            if (!string.IsNullOrEmpty(entryName))
            {
                AddVerifyEntry(fileName, entryName);
            }
            if (!_items.TryGetValue(fileName, out var item))
            {
                _items.Add(fileName, item = new());
            }
            dependencyFileName = BundleStorage.Separate(dependencyFileName, out entryName);
            if (!string.IsNullOrEmpty(entryName))
            {
                AddVerifyEntry(dependencyFileName, entryName);
                item.AddLink(entryName);
            }
            item.AddLink(dependencyFileName);
        }

        public void AddDependencyEntry(string fileName, long dependencyEntryId)
        {
            if (_writer is null)
            {
                return;
            }
            fileName = BundleStorage.Separate(fileName, out var entryName);
            if (!string.IsNullOrEmpty(entryName))
            {
                AddVerifyEntry(fileName, entryName);
            }
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
            fileName = BundleStorage.Separate(fileName, out var entryName);
            if (!string.IsNullOrEmpty(entryName))
            {
                AddVerifyEntry(fileName, entryName);
            }
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
            fileName = BundleStorage.Separate(fileName, out var entryName);
            if (!string.IsNullOrEmpty(entryName))
            {
                AddVerifyEntry(fileName, entryName);
            }
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
            fileName = BundleStorage.Separate(fileName, out var entry);
            if (!string.IsNullOrEmpty(entry))
            {
                AddVerifyEntry(fileName, entry);
            }
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
            fileName = BundleStorage.Separate(fileName, out var entryName);
            if (!string.IsNullOrEmpty(entryName))
            {
                AddVerifyEntry(fileName, entryName);
            }
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
            fileName = BundleStorage.Separate(fileName, out var entryName);
            if (!string.IsNullOrEmpty(entryName))
            {
                AddVerifyEntry(fileName, entryName);
            }
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
            fileName = BundleStorage.Separate(fileName, out var entry);
            if (!string.IsNullOrEmpty(entry))
            {
                AddVerifyEntry(fileName, entry);
            }
            if (!_items.TryGetValue(fileName, out var item))
            {
                _items.Add(fileName, item = new());
            }
            item.Add(entryId, entryName, entryType);
        }

        public void AddEntry(string fileName, string entryName)
        {
            if (_writer is null)
            {
                return;
            }
            fileName = BundleStorage.Separate(fileName, out var entry);
            if (!string.IsNullOrEmpty(entry))
            {
                AddVerifyEntry(fileName, entry);
            }
            if (!_items.TryGetValue(fileName, out var item))
            {
                _items.Add(fileName, item = new());
            }
            item.Add(entryName);
        }
        private void AddVerifyEntry(string fileName, string entryName)
        {
            if (!_items.TryGetValue(fileName, out var item))
            {
                _items.Add(fileName, item = new());
            }
            item.Add(entryName);
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
            var res = new DependencyDictionary();
            foreach (var item in _items)
            {
                var items = new List<string>();
                foreach (var target in _items)
                {
                    if (item.Key == target.Key)
                    {
                        continue;
                    }
                    if (item.Value.Contains(target.Value) || target.Value.Contains(item.Value))
                    {
                        items.Add(target.Key);
                    }
                }
                res.Add(item.Key, [..items]);
            }
            return res;
        }

        public static DependencyBuilder Load(Stream input)
        {
            var builder = new DependencyBuilder();
            var reader = new BinaryReader(input);
            while (input.Position < input.Length)
            {
                var fileName = reader.ReadString();
                var item = new DependencyEntry();
                item.Read(reader);
                builder._items.TryAdd(fileName, item);
            }
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

            public bool Contains(DependencyEntry target)
            {
                if (this == target)
                {
                    return false;
                }
                foreach (var item in target._linkedPart)
                {
                    if (Contains(item))
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
                    writer.Write(item.Name);
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
    }
}
