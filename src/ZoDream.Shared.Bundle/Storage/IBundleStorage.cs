using System.IO;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.Bundle
{
    public interface IBundleStorage
    {
        public IBundleBinaryReader OpenRead(string fullPath);
        public IBundleBinaryReader OpenRead(Stream input, IFilePath sourcePath);
    }
}
