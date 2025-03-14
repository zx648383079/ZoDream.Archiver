using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            : this(File.Create(fullPath))
        {
            
        }

        public DependencyBuilder(Stream output)
        {
            _writer = new BinaryWriter(output, Encoding.UTF8);
        }

        private readonly BinaryWriter? _writer;

        public void AddDependency(string fileName, string dependencyFileName)
        {
        }

        public void AddDependencyEntry(string fileName, long dependencyEntryId)
        {
        }

        public void AddDependencyEntry(string fileName, long entryId, long dependencyEntryId)
        {
        }

        public void AddDependencyEntry(string fileName, string entryName, string dependencyEntryName)
        {
        }

        public void AddDependencyEntry(string fileName, string dependencyEntryName)
        {
        }

        public void AddEntry(string fileName, long entryId)
        {
        }

        public void AddEntry(string fileName, string entryName)
        {
        }

        public void Flush()
        {
        }

        public void Dispose()
        {
            _writer?.Dispose();
        }
    }
}
