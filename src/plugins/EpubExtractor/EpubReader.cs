using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.EpubExtractor
{
    public class EpubReader(Stream stream, IArchiveOptions options) : IArchiveReader
    {
        

        public void ExtractTo(IReadOnlyEntry entry, Stream output)
        {
            throw new NotImplementedException();
        }

        public void ExtractToDirectory(string folder, ArchiveExtractMode mode, Action<double>? progressFn = null, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IReadOnlyEntry> ReadEntry()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            if (options?.LeaveStreamOpen == false)
            {
                stream.Dispose();
            }
        }
    }
}
