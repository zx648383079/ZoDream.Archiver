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
    public class BlowfishReader(IEnumerable<string> fileItems) : IBundleReader
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
                if (!IsSupport(fs, out var extension))
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

        private static bool IsSupport(Stream input, out string extension)
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
            var res = SignatureEq(CONST_IMG, buffer) || SignatureEq(CONST_LUA, buffer);
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
            if (SignatureEq(CONST_IMG, buffer))
            {
                format = FileFormat.IMAGE;
            }
            else if (SignatureEq(CONST_LUA, buffer))
            {
                format = FileFormat.LUA; 
            }
            input.Seek(0, SeekOrigin.Begin);
            if (format == FileFormat.None)
            {
                input.CopyTo(output);
                return;
            }
            var key = TryGetKey(buffer);
            if (string.IsNullOrWhiteSpace(key))
            {
                // 不能加密
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

        private string TryGetKey(byte[] buffer)
        {
            if (buffer[2] == 'o' && buffer[3] == 'i')
            {
                return "fd1c1b2f34a0d1d246be3ba9bc5af022e83375f315a0216085d3013a";
            }
            if (buffer[2] == 'r' && buffer[3] == 'c')
            {

            }
            return string.Empty;
        }

        private static bool SignatureEq(int[] signature, byte[] data)
        {
            if (signature.Length > data.Length)
            {
                return false;
            }
            for (var i = 0; i < signature.Length; i++)
            {
                if (signature[i] >= 0 && signature[i] != data[i])
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
