using System;
using System.IO;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    public class IceConeElementScanner(string package) : IBundleParser
    {
        public bool IsAeonFantasy => package.Contains("aeon");

        public IBundleBinaryReader OpenRead(string fullPath)
        {
            return Parse(File.OpenRead(fullPath), FilePath.Parse(fullPath));
        }

        public IBundleBinaryReader Parse(Stream input, IFilePath sourcePath)
        {
            if (input is EeabStream)
            {
                return new BundleBinaryReader(input, EndianType.BigEndian);
            }
            var pos = input.Position;
            var buffer = input.ReadBytes(4);
            input.Seek(pos, SeekOrigin.Begin);
            if (buffer.Equal("eeab") && input.Length > 8)
            {
                return new BundleBinaryReader(new EeabStream(input, FilePath.GetFilePath(sourcePath)), EndianType.BigEndian);
            }
            if (buffer.Equal("euab"))
            {
                return new BundleBinaryReader(input.Skip(EeabStream.EuabParse(
                    FilePath.GetFilePath(sourcePath))), EndianType.BigEndian);
            }
            return new BundleBinaryReader(input, EndianType.BigEndian);
        }
    }
}
