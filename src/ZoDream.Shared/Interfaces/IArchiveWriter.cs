using System;
using System.IO;

namespace ZoDream.Shared.Interfaces
{
    public interface IArchiveWriter: IDisposable
    {

        public IReadOnlyEntry AddEntry(string name, string fullPath);
        public IReadOnlyEntry AddEntry(string name, Stream input);
    }
}
