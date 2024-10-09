using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.Shared.Interfaces;

namespace ZoDream.PdfInserter
{
    public class PdfWriter(Stream stream, IArchiveOptions options) : IArchiveWriter
    {
        public IReadOnlyEntry AddEntry(string name, string fullPath)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyEntry AddEntry(string name, Stream input)
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
