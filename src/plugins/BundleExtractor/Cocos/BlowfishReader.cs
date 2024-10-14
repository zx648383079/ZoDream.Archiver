using System.Collections.Generic;
using System.IO;
using System.Threading;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.RustWrapper;

namespace ZoDream.BundleExtractor.Cocos
{
    public class BlowfishReader(IEnumerable<string> fileItems, string key) : IBundleReader
    {
        private static readonly int[] CONST_IMG = [ 'd', 'r', 'o', 'i', 'd', 'p', 'n', 'g' ];
        private static readonly int[] CONST_LUA = [ 'd', 'r', 'o', 'i', -1, 'l', 'u', 'a' ];

        public void ExtractTo(string folder, ArchiveExtractMode mode, CancellationToken token = default)
        {
            foreach (var item in fileItems)
            {
                
            }
        }

        private void Extract(Stream input, Stream output)
        {
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
