using System.IO;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.Shared.Bundle
{
    public partial class BundleStorage : IBundleStorage
    {
        public IBundleBinaryReader OpenRead(string fullPath)
        {
            return OpenRead(File.OpenRead(fullPath), new FilePath(fullPath));
        }

        public IBundleBinaryReader OpenRead(Stream input, IFilePath sourcePath)
        {
            return new BundleBinaryReader(input, EndianType.LittleEndian);
        }
    }
}
