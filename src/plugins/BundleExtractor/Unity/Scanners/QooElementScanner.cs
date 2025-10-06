using System;
using System.IO;
using System.IO.Compression;
using System.IO.Enumeration;
using System.Security.Cryptography;
using System.Text;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    internal class QooElementScanner : IBundleParser
    {
        public IBundleBinaryReader OpenRead(string fullPath)
        {
            var input = File.OpenRead(fullPath);
            return Parse(input, new FilePath(fullPath));
        }

        public IBundleBinaryReader Parse(Stream input, IFilePath sourcePath)
        {
            if (sourcePath.Name == "spine.bundle")
            {
                input = new XORStream(input, [0xFF], 64);
            }
            if (FileSystemName.MatchesSimpleExpression("dialogue*.json", sourcePath.Name, true))
            {

            }
            return new BundleBinaryReader(input, EndianType.BigEndian);
        }

        private static Stream Decrypt(Stream input)
        {
            using var reader = new StreamReader(input);
            var sb = new StringBuilder();
            while (true)
            {
                var line = reader.ReadLine();
                if (line == null)
                {
                    break;
                }
                sb.Append(line.Trim());
            }
            var data = Convert.FromBase64String(sb.ToString());
            using var cipher = new AesGcm(
                [.. Convert.FromBase64String("/I0NTvnHjOjaxyWwy36I6g=="), .. data[..16]], 16);
            var ms = new MemoryStream(data.Length - 44);
            cipher.Decrypt(data[16..28], data[44..], data[28..44], ms.GetBuffer(),
                Encoding.UTF8.GetBytes(
                Convert.ToHexStringLower(MD5.HashData(data[..28]))));
            return new ZLibStream(ms, CompressionMode.Decompress, false);
        }
    }
}
