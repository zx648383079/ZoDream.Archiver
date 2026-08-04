using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Threading;
using ZoDream.Shared;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.RenPy
{
    public class RenPyReader(IBundleBinaryReader reader, string fileName, IEntryService service, IBundleOptions options) : IBundleHandler
    {

        private static readonly string Signature = "RPA-";



        public void ExtractTo(string folder, ArchiveExtractMode mode, CancellationToken token = default)
        {
            var buffer = new byte[8];
            reader.Read(buffer, 0, 2);
            if (buffer[0] == 0x78 && buffer[1] == 0x9c)
            {
                // V1
                new ZLibStream(reader.ReadAsStream(reader.RemainingLength), 
                    CompressionMode.Decompress);
            } else if (buffer[0] == 'R' && buffer[1] == 'P')
            {
                reader.Read(buffer, 2, 6);
                var version = buffer[4]; // 2 || 3

                var offset = int.Parse(reader.ReadString(16), NumberStyles.HexNumber);
                if (version > 2)
                {
                    var key = int.Parse(reader.ReadString(8), NumberStyles.HexNumber);
                    reader.Position += 7;
                }
                reader.BaseStream.Seek(offset, SeekOrigin.Begin);
                new ZLibStream(reader.ReadAsStream(reader.RemainingLength),
                    CompressionMode.Decompress);
                if (version > 2)
                {
                    //for k in index.keys():
                    //if len(index[k][0]) == 2:
                    //    index[k] = [(offset ^ key, dlen ^ key) for offset, dlen in index[k]]
                    //else:
                    //    index[k] = [(offset ^ key, dlen ^ key, start_to_bytes(start)) for offset, dlen, start in index[k]]
                }
            }
            else
            {
                throw new SignatureException("Unknown renpy format");
            }
        }

        public void Dispose()
        {
            reader.Dispose();
        }
    }
}