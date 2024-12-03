using System.IO;
using ZoDream.Shared.Models;

namespace ZoDream.Shared.Bundle
{
    public class BundleStorage : IBundleStorage
    {
        public Stream Open(string path)
        {
            return File.OpenRead(path);
        }

        public IBundleBinaryReader OpenRead(string path)
        {
            return OpenRead(Open(path));
        }

        public IBundleBinaryReader OpenRead(Stream input)
        {
            return new BundleBinaryReader(input, EndianType.LittleEndian);
        }
    }
}
