using System;
using System.IO;
using System.Linq;
using System.Text;
using ZoDream.BundleExtractor.Unity.BundleFiles;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    public class AisnoElementScanner(string package) : IBundleStorage
    {
        public bool IsWQMT => package.Contains("wqmt");

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
            if (input is XORStream)
            {
                return new BundleBinaryReader(input, EndianType.BigEndian);
            }
            input.Position = 50;
            var buffer = input.ReadBytes(8);
            var xorKey = buffer[7];
            if (buffer.Take(7).Select(i => (byte)(i ^ xorKey)).SequenceEqual(
                Encoding.ASCII.GetBytes(FileStreamBundleHeader.UnityFSMagic)))
            {
                return new BundleBinaryReader(new XORStream(
                    input.Skip(50),
                    [xorKey]
                ), EndianType.BigEndian);
            }
            return new BundleBinaryReader(input, EndianType.BigEndian);
        }
    }
}
