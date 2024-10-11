using System.IO;
using System;

namespace ZoDream.Shared.RustWrapper
{
    public unsafe class Compressor : IDisposable
    {
        public Compressor(CompressionID compression)
        {
            _instance = NativeMethods.find_compressor(compression);
        }

        private readonly CompressorRef* _instance;

        public byte[] Compress(byte[] buffer)
        {
            return Encryptor.Convert(buffer, (inputRef, outputRef) => {
                return NativeMethods.compress_compressor(_instance, &inputRef, &outputRef);
            });
        }

        public void Compress(Stream input, Stream output)
        {
            Encryptor.Convert(input, output, (inputRef, outputRef) => {
                return NativeMethods.compress_compressor(_instance, &inputRef, &outputRef);
            });
        }

        public byte[] Decompress(byte[] buffer)
        {
            return Encryptor.Convert(buffer, (inputRef, outputRef) => {
                return NativeMethods.decompress_compressor(_instance, &inputRef, &outputRef);
            });
        }
        public void Decompress(Stream input, Stream output)
        {
            Encryptor.Convert(input, output, (inputRef, outputRef) => {
                return NativeMethods.decompress_compressor(_instance, &inputRef, &outputRef);
            });
        }

        public void Dispose()
        {
            NativeMethods.free_compressor(_instance);
        }
    }

}
