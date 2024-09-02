using System;
using System.IO;

namespace ZoDream.Shared.Compression.Own
{
    public interface IOwnKey : IDisposable
    {
        public void Seek(long len, SeekOrigin origin);

        public byte ReadByte();
    }
}
