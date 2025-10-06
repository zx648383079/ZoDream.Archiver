using System.IO;
using System.Text;
using System.Threading;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.RustWrapper;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.Cocos
{
    public class BlowfishReader(IBundleChunk fileItems) : IBundleHandler
    {
        private static readonly int[] CONST_IMG = [ 'd', 'r', -1, -1, -1, 'p', 'n', 'g' ];
        private static readonly int[] CONST_LUA = [ 'd', 'r', -1, -1, -1, 'l', 'u', 'a' ];

        public void ExtractTo(string folder, ArchiveExtractMode mode, CancellationToken token = default)
        {
            foreach (var item in fileItems.Items)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                var fileName = fileItems.Create(item, folder);
                using var fs = fileItems.OpenRead(item);
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
            input.ReadExactly(buffer, 0, 8);
            //input.Seek(0, SeekOrigin.Begin);
            var res = buffer.StartsWith(CONST_IMG) || buffer.StartsWith(CONST_LUA);
            extension = res ? "." + Encoding.ASCII.GetString(buffer, 5, 3).ToLower() : string.Empty;
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
            input.ReadExactly(buffer, 0, 8);
            var format = FileFormat.None;
            if (buffer.StartsWith(CONST_IMG))
            {
                format = FileFormat.IMAGE;
            }
            else if (buffer.StartsWith(CONST_LUA))
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
            if (key.Length == 0)
            {
                // 不能解密
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

        private byte[] TryGetKey(byte[] buffer)
        {
            if (true || (buffer[2] == 'o' && buffer[3] == 'i'))
            {
                return Encoding.ASCII.GetBytes("fd1c1b2f34a0d1d246be3ba9bc5af022e83375f315a0216085d3013a");
            }
            return [];
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
