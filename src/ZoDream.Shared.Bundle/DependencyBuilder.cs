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

        private readonly HashSet<int> _fileItems = [];

        public void AddDependency(string fileName, string dependencyFileName)
        {
            if (_writer is null)
            {
                return;
            }
            var hash = BundleStorage.ToHashCode(fileName);
            if (_fileItems.Contains(hash))
            {
                return;
            }
            _writer.Write(dependencyFileName);
        }

        public void AddDependencyEntry(string fileName, long dependencyEntryId)
        {
            if (_writer is null)
            {
                return;
            }
        }

        public void AddDependencyEntry(string fileName, long entryId, long dependencyEntryId)
        {
            if (_writer is null)
            {
                return;
            }
        }
        public void AddDependencyEntry(string fileName, long entryId, string dependencyEntryName)
        {
            if (_writer is null)
            {
                return;
            }
        }
        public void AddDependencyEntry(string fileName, string entryName, string dependencyEntryName)
        {
            if (_writer is null)
            {
                return;
            }
        }

        public void AddDependencyEntry(string fileName, string dependencyEntryName)
        {
            if (_writer is null)
            {
                return;
            }
        }

        public void AddEntry(string fileName, long entryId)
        {
            if (_writer is null)
            {
                return;
            }
        }

        public void AddEntry(string fileName, string entryName)
        {
            if (_writer is null)
            {
                return;
            }
        }

        public void Flush()
        {
            if (_writer is null)
            {
                return;
            }
            _writer.Flush();
        }

        public void Dispose()
        {
            _writer?.Dispose();
        }

    }
}
