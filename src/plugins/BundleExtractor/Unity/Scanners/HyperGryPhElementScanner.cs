using System.IO;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    public partial class HyperGryPhElementScanner(string package) : IBundleParser
    {

        public bool IsArkNightsEndfield => package.Contains("endfield");

        public bool IsExAstris => package.Contains("exa");


        public IBundleBinaryReader OpenRead(string fullPath)
        {
            return Parse(File.OpenRead(fullPath), new FilePath(fullPath));
        }

        public IBundleBinaryReader Parse(Stream input, IFilePath sourcePath)
        {
            return new BundleBinaryReader(input, EndianType.BigEndian);
        }
        
    }
}
