using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.CompressedFiles
{
    public class GZipFileScheme : IArchiveScheme
    {
        public IArchiveWriter Create(Stream stream, IArchiveOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public bool IsReadable(Stream stream)
        {
            if (stream.Length - stream.Position < 2)
            {
                return false;
            }
            var pos = stream.Position;
            var reader = new EndianReader(stream, EndianType.BigEndian);
            try
            {
                return reader.ReadUInt16() == 0x1F8B;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            } finally
            {
                stream.Position = pos;
            }
        }

        public IArchiveReader? Open(Stream stream, string filePath, string fileName, IArchiveOptions? options = null)
        {
            if (!IsReadable(stream))
            {
                return null;
            }
            return new StreamArchiveReader(fileName,
                new GZipStream(stream, CompressionMode.Decompress, options?.LeaveStreamOpen != false),
                options);
        }
    }
}
