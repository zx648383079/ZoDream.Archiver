using System.Diagnostics;
using System.IO;
using System;
using System.Runtime.InteropServices;

namespace ZoDream.Shared.RustWrapper
{
    public unsafe class Painter : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pixel"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="blockWidth">一些其他配置</param>
        /// <param name="blockHeight">一些其他的配置</param>
        public Painter(
            PixelID pixel, 
            int width, int height,
            int blockWidth = 0, int blockHeight = 0)
        {
            _instance = NativeMethods.find_painter(pixel, 
                (uint)width, (uint)height,
                (uint)blockWidth,
                (uint)blockHeight
            );
        }

        private readonly PainterRef* _instance;
        private LoggerRef _logger = new()
        {
            log = (byte* msgPtr) => {
                var msg = Marshal.PtrToStringUTF8((nint)msgPtr);
                Debug.WriteLine($"logger call: {msg} ");
            },
            progress = (uint progress, uint total, byte* msgPtr) => {
                var msg = Marshal.PtrToStringUTF8((nint)msgPtr);
                Debug.WriteLine($"logger call: {progress}/{total}; {msg} ");
            }
        };
        public byte[] Encode(byte[] buffer)
        {
            return Encode(buffer, buffer.Length);
        }
        public byte[] Encode(byte[] input, int inputLength)
        {
            return Encryptor.Convert(input, inputLength, (inputRef, outputRef) => {
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
            return Decode(buffer, buffer.Length);
        }
        public byte[] Decode(byte[] input, int inputLength)
        {
            return Encryptor.Convert(input, inputLength, (inputRef, outputRef) => {
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
