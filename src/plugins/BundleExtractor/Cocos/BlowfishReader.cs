using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.RustWrapper;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.Cocos
{
    public class BlowfishReader(IEnumerable<string> fileItems, string key) : IBundleReader
    {
        private static readonly int[] CONST_IMG = [ 'd', 'r', -1, -1, -1, 'p', 'n', 'g' ];
        private static readonly int[] CONST_LUA = [ 'd', 'r', -1, -1, -1, 'l', 'u', 'a' ];

        public void ExtractTo(string folder, ArchiveExtractMode mode, CancellationToken token = default)
        {
            foreach (var item in fileItems)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                var fileName = fileItems is IBundleChunk c ? c.Create(item, folder) 
                    : Path.Combine(folder, Path.GetFileName(item));
                using var fs = File.OpenRead(item);
                if (!IsSupprot(fs, out var extension))
                {
                    continue;
                }
                if (!LocationStorage.TryCreate(fileName, extension, mode, out fileName))
                {
                    continue;
                }
                using var output = File.Create(fileName);
                Extract(fs, output);
            }
        }

        private static bool IsSupprot(Stream input, out string extension)
        {
            if (input.Length <= 8)
            {
                extension = string.Empty;
                return false;
            }
            var buffer = new byte[8];
            input.Seek(-8, SeekOrigin.End);
            input.Read(buffer, 0, 8);
            //input.Seek(0, SeekOrigin.Begin);
            var res = SignureEq(CONST_IMG, buffer) || SignureEq(CONST_LUA, buffer);
            extension = res ? Encoding.ASCII.GetString(buffer, 5, 3).ToLower() : string.Empty;
            return res;
        }

        private void Extract(Stream input, Stream output)
        {
            if (input.Length <= 8)
            {
                input.CopyTo(output);
                return;
            }
            var buffer = new byte[8];
            input.Seek(-8, SeekOrigin.End);
            input.Read(buffer, 0, 8);
            var format = FileFormat.None;
            if (SignureEq(CONST_IMG, buffer))
            {
                format = FileFormat.IMAGE;
            }
            else if (SignureEq(CONST_LUA, buffer))
            {
                format = FileFormat.LUA; 
            }
            input.Seek(0, SeekOrigin.Begin);
            if (format == FileFormat.None)
            {
                input.CopyTo(output);
                return;
            }
            var length = input.Length - 8;
            using var encryptor = new Encryptor(format == FileFormat.IMAGE ?
                EncryptionID.Blowfish : EncryptionID.BlowfishCBC, key);
            encryptor.Decrypt(new PartialStream(input, length), output);
            if (format == FileFormat.LUA)
            {
                output.SetLength(length - buffer[4]);
            }
        }

        private static bool SignureEq(int[] signure, byte[] data)
        {
            if (signure.Length > data.Length)
            {
                return false;
            }
            for (var i = 0; i < signure.Length; i++)
            {
                if (signure[i] >= 0 && signure[i] != data[i])
                {
                    return false;
                }
            }
            return true;
        }

        public void Dispose()
        {

        }

        private enum FileFormat
        {
            None,
            IMAGE,
            LUA
        }
    }
}
