using System;
using System.IO;
using System.IO.Compression;
using System.IO.Enumeration;
using System.Security.Cryptography;
using System.Text;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    public class QooElementScanner(IBundleSource source, IBundleExtractOptions options) : IBundleParser, IBundleFilter
    {
        public bool IsMatch(IFilePath filePath)
        {
            if (!FileSystemName.MatchesSimpleExpression("dialogue*.json", filePath.Name, true))
            {
                return false;
            }
            var fileName = BundleStorage.Create(source, filePath, string.Empty, options.OutputFolder);
            if (LocationStorage.TryCreate(fileName, ".json", options.FileMode, out fileName))
            {
                using var fs = Decrypt(source.OpenRead(filePath));
                fs.SaveAs(fileName);
            }
            return true;
        }

        public IBundleBinaryReader Parse(Stream input, IFilePath sourcePath)
        {
            var pos = input.Position;
            if (input.ReadByte() == 0xAA)
            {
                input = new XORStream(input, [0xFF], 0x3f);
            }
            input.Position = pos;
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
