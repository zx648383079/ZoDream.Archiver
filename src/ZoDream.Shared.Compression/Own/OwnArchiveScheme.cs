﻿using System;
using System.IO;
using System.Threading.Tasks;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.Compression.Own
{
    public class OwnArchiveScheme : IArchiveScheme
    {
        public IArchiveWriter Create(Stream stream, IArchiveOptions? options = null)
        {
            return new OwnArchiveWriter(stream, options);
        }

        public bool IsReadable(Stream stream)
        {
            return IsSupport(stream) is not null;
        }

        public IArchiveReader? Open(Stream stream, string filePath, string fileName, IArchiveOptions? options = null)
        {
            var header = IsSupport(stream);
            if (header is null)
            {
                return null;
            }
            return new OwnArchiveReader(stream, options);
        }

        public Task<IArchiveReader?> OpenAsync(Stream stream, string filePath, string fileName, IArchiveOptions? options = null)
        {
            return Task.FromResult(Open(stream, filePath, fileName, options));
        }

        public Task<IArchiveWriter> CreateAsync(Stream stream, IArchiveOptions? options = null)
        {
            return Task.FromResult(Create(stream, options));
        }

        public static IOwnKey CreateKey(IArchiveOptions options)
        {
            if (options?.Dictionary is null)
            {
                throw new CryptographicException("dictionary Path is must");
            }
            var bin = new OwnDictionary(File.OpenRead(options.Dictionary));
            if (string.IsNullOrWhiteSpace(options.Password))
            {
                return bin;
            }
            return new OwnMultipleKey(bin, new OwnPassword(options.Password));
        }

        public static OwnFileHeader? IsSupport(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            try
            {
                var header = new OwnFileHeader();
                header.Read(stream);
                return header;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                stream.Seek(0, SeekOrigin.Begin);
            }
        }
    }
}
