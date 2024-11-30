using System;
using System.IO;
using System.Text.RegularExpressions;
using ZoDream.Shared.Interfaces;
using System.Collections.Concurrent;

namespace ZoDream.Shared.IO
{
    public partial class TemporaryStorage(string folder) : ITemporaryStorage
    {
        public TemporaryStorage()
            : this(AppDomain.CurrentDomain.BaseDirectory)
        {
            
        }

        private readonly ConcurrentQueue<string> _fileItems = [];

        public Stream Create()
        {
            var fullPath = Path.Combine(folder, Guid.NewGuid().ToString());
            _fileItems.Enqueue(fullPath);
            return File.Create(fullPath, 1024, FileOptions.DeleteOnClose);
        }

        public Stream Create(string guid)
        {
            var fullPath = Path.Combine(folder, SafePathRegex().Replace(guid, "_"));
            _fileItems.Enqueue(fullPath);
            return File.Create(fullPath, 1024, FileOptions.DeleteOnClose);
        }

        public void Clear()
        {
        }

        public void Dispose()
        {
            while (_fileItems.TryDequeue(out var fileName))
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }
        }

        [GeneratedRegex(@"[\\/.:\<\>\(\)]")]
        private static partial Regex SafePathRegex();

        
    }
}
