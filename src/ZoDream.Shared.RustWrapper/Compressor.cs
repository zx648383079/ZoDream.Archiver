using System.IO;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.RustWrapper
{
    public unsafe class Compressor : ICompressor, IDecompressor, IDisposable
    {
        public Compressor(CompressionID compression)
        {
            _instance = NativeMethods.find_compressor(compression);
        }

        private readonly CompressorRef* _instance;
        private LoggerRef _logger = new()
        {
            log = (byte* msgPtr) => {
                var msg = Marshal.PtrToStringUTF8((IntPtr)msgPtr);
                Debug.WriteLine($"logger call: {msg} ");
            },
            progress = (uint progress, uint total, byte* msgPtr) => {
                var msg = Marshal.PtrToStringUTF8((IntPtr)msgPtr);
                Debug.WriteLine($"logger call: {progress}/{total}; {msg} ");
            }
        };

        public byte[] Compress(byte[] buffer)
        {
            return Encryptor.Convert(buffer, (inputRef, outputRef) => {
                return NativeMethods.compress_compressor(_instance, ref inputRef, ref outputRef, ref _logger);
            });
        }

        public void Compress(Stream input, Stream output)
        {
            Encryptor.Convert(input, output, (inputRef, outputRef) => {
                return NativeMethods.compress_compressor(_instance, ref inputRef, ref outputRef, ref _logger);
            });
        }

        public byte[] Decompress(byte[] buffer)
        {
            return Encryptor.Convert(buffer, (inputRef, outputRef) => {
                return NativeMethods.decompress_compressor(_instance, ref inputRef, ref outputRef, ref _logger);
            });
        }
        public void Decompress(Stream input, Stream output)
        {
            Encryptor.Convert(input, output, (inputRef, outputRef) => {
                return NativeMethods.decompress_compressor(_instance, ref inputRef, ref outputRef, ref _logger);
            });
        }

        public void Dispose()
        {
            NativeMethods.free_compressor(_instance);
        }
    }

}
