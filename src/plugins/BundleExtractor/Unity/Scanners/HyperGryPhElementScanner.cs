using System.IO;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    public partial class HyperGryPhElementScanner(string package) : IBundleStorage
    {

        public bool IsArkNightsEndfield => package.Contains("endfield");

        public bool IsExAstris => package.Contains("exa");


        public IBundleBinaryReader OpenRead(string fullPath)
        {
            return OpenRead(File.OpenRead(fullPath), new FilePath(fullPath));
        }

        public IBundleBinaryReader OpenRead(Stream input, IFilePath sourcePath)
        {
            return new BundleBinaryReader(input, EndianType.BigEndian);
        }
        
    }
}
