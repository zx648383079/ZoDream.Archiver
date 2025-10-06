using System.IO;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.Shared.Bundle
{
    public partial class BundleStorage : IBundleParser
    {

        public IBundleBinaryReader OpenRead(string fullPath)
        {
            return Parse(File.OpenRead(fullPath), new FilePath(fullPath));
        }


        public IBundleBinaryReader Parse(Stream input, IFilePath sourcePath)
        {
            return new BundleBinaryReader(input, EndianType.LittleEndian);
        }
    }
}
