using System.Diagnostics;
using System.IO;
using System;
using System.Runtime.InteropServices;

namespace ZoDream.Shared.RustWrapper
{
    public unsafe class Painter : IDisposable
    {
        public Painter(PixelID pixel, int width, int height)
        {
            _instance = NativeMethods.find_painter(pixel, (uint)width, (uint)height);
        }

        private readonly PainterRef* _instance;
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

        public byte[] Encode(byte[] buffer)
        {
            return Encryptor.Convert(buffer, (inputRef, outputRef) => {
                return NativeMethods.encode_painter(_instance, ref inputRef, ref outputRef, ref _logger);
            });
        }

        public void Encode(Stream input, Stream output)
        {
            Encryptor.Convert(input, output, (inputRef, outputRef) => {
                return NativeMethods.encode_painter(_instance, ref inputRef, ref outputRef, ref _logger);
            });
        }

        public byte[] Decode(byte[] buffer)
        {
            return Encryptor.Convert(buffer, (inputRef, outputRef) => {
                return NativeMethods.decode_painter(_instance, ref inputRef, ref outputRef, ref _logger);
            });
        }
        public void Decode(Stream input, Stream output)
        {
            Encryptor.Convert(input, output, (inputRef, outputRef) => {
                return NativeMethods.decode_painter(_instance, ref inputRef, ref outputRef, ref _logger);
            });
        }

        public void Dispose()
        {
            NativeMethods.free_painter(_instance);
        }
    }

}
