using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.CompressedFiles
{
    public class GZipFileScheme : IArchiveScheme, IBundleArchiveScheme
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

            try
            {
                return stream.ReadByte() == 0x1F && stream.ReadByte() == 0x8B;
            }
            catch (Exception ex)
            {
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

        public IArchiveReader? Open(IBundleBinaryReader reader, string filePath, string fileName, IArchiveOptions? options = null)
        {
            return Open(reader.BaseStream, filePath, fileName, options);
        }
    }
}
