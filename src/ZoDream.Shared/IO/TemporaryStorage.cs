using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.IO
{
    public partial class TemporaryStorage(string folder) : ITemporaryStorage
    {
        public TemporaryStorage()
            : this(AppDomain.CurrentDomain.BaseDirectory)
        {
            
        }

        public Stream Create()
        {
            var fullPath = Path.Combine(folder, Guid.NewGuid().ToString());
            return File.Create(fullPath, 1024, FileOptions.DeleteOnClose);
        }

        public Stream Create(string guid)
        {
            var fullPath = Path.Combine(folder, SafePathRegex().Replace(guid, "_"));
            return File.Create(fullPath, 1024, FileOptions.DeleteOnClose);
        }

        public void Dispose()
        {
        }

        [GeneratedRegex(@"[\\/.:\<\>\(\)]")]
        private static partial Regex SafePathRegex();
    }
}
