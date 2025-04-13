using System.IO;
using ZoDream.Shared.Models;

namespace ZoDream.Shared.Bundle
{
    public partial class BundleStorage : IBundleStorage
    {
        public Stream Open(string fullPath)
        {
            return File.OpenRead(fullPath);
        }

        public IBundleBinaryReader OpenRead(string fullPath)
        {
            return OpenRead(Open(fullPath), fullPath);
        }

        public IBundleBinaryReader OpenRead(Stream input, string fileName)
        {
            return new BundleBinaryReader(input, EndianType.LittleEndian);
        }
    }
}
