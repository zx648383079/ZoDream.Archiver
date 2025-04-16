using System.Collections.Generic;
using System.IO;
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
            item.Add(entryId);
            item.Add(entryName);
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


        private class DependencyEntry
        {
            private readonly HashSet<long> _children = [];
            private readonly HashSet<string> _children2 = [];
            private readonly HashSet<long> _linked = [];
            private readonly HashSet<string> _linked2 = [];

            public void Add(string child)
            {
                if (string.IsNullOrWhiteSpace(child))
                {
                    return;
                }
                _children2.Add(child);
            }

            public void Add(long child)
            {
                if (child == 0)
                {
                    return;
                }
                _children.Add(child);
            }

            public void AddLink(string link)
            {
                if (string.IsNullOrWhiteSpace(link))
                {
                    return;
                }
                if (_children2.Contains(link))
                {
                    return;
                }
                _linked2.Add(link);
            }

            public void AddLink(long link)
            {
                if (link == 0)
                {
                    return;
                }
                if (_children.Contains(link))
                {
                    return;
                }
                _linked.Add(link);
            }

            public void Write(BinaryWriter writer)
            {
                writer.Write(_children.Count);
                foreach (var item in _children)
                {
                    writer.Write(item);
                }
                writer.Write(_children2.Count);
                foreach (var item in _children2)
                {
                    writer.Write(item);
                }
                writer.Write(_linked.Count);
                foreach (var item in _linked)
                {
                    writer.Write(item);
                }
                writer.Write(_linked2.Count);
                foreach (var item in _linked2)
                {
                    writer.Write(item);
                }
            }
        }
    }
}
