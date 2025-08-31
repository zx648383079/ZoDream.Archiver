using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.RustWrapper
{
    public unsafe class Encryptor: IEncryptCipher, IDecryptCipher, IDisposable
    {
        public Encryptor(EncryptionID encryption)
        {
            _instance = NativeMethods.find_encryptor(encryption);
        }

        public Encryptor(EncryptionID encryption, string key)
        {
            var ptr = Marshal.StringToCoTaskMemUTF8(key);
            _instance = NativeMethods.find_encryptor(encryption, (byte*)ptr);
            Marshal.FreeCoTaskMem(ptr);
        }

        public Encryptor(EncryptionID encryption, byte[] key)
        {
            fixed(byte* ptr = key)
            {
                _instance = NativeMethods.find_encryptor(encryption, ptr);
            }
        }

        private readonly EncryptorRef* _instance;
        private LoggerRef _logger = new()
        {
            log = (byte* msgPtr) => {
                var msg = Marshal.PtrToStringUTF8((nint)msgPtr);
                Debug.WriteLine($"logger call: {msg} ");
            },
            progress = (uint progress, uint total, byte * msgPtr) =>
            {
                var msg = Marshal.PtrToStringUTF8((nint)msgPtr);
                Debug.WriteLine($"logger call: {progress}/{total}; {msg} ");
            }
        };

        public byte[] Encrypt(byte[] buffer)
        {
            return Encrypt(buffer, buffer.Length);
        }

        public byte[] Encrypt(byte[] input, int inputLength)
        {
            return Convert(input, inputLength, (inputRef, outputRef) => {
                return NativeMethods.encrypt_encryptor(_instance, ref inputRef,
                    ref outputRef, ref _logger);
            });
        }

        public void Encrypt(Stream input, Stream output)
        {
            Convert(input, output, (inputRef, outputRef) => {
                return NativeMethods.encrypt_encryptor(_instance,
                    ref inputRef, ref outputRef, ref _logger);
            });
        }

        public byte[] Decrypt(byte[] input)
        {
            return Decrypt(input, input.Length);
        }
        public byte[] Decrypt(byte[] input, int inputLength)
        {
            return Convert(input, inputLength, (inputRef, outputRef) => {
                return NativeMethods.decrypt_encryptor(_instance, ref inputRef, ref outputRef, ref _logger);
            });
        }
        public void Decrypt(Stream input, Stream output)
        {
            Convert(input, output, (inputRef, outputRef) => {
                return NativeMethods.decrypt_encryptor(_instance, ref inputRef, ref outputRef, ref _logger);
            });
        }

        public void Dispose()
        {
            NativeMethods.free_encryptor(_instance);
        }


        public static byte[] Convert(byte[] buffer, int bufferLength, Func<InputStreamRef, OutputStreamRef, long> cb)
        {
            var i = 0;
            var inputRef = new InputStreamRef()
            {
                read = (byte* ptr, uint count) => {
                    var len = Math.Min(bufferLength - i, (int)count);
                    if (len > 0)
                    {
                        Marshal.Copy(buffer, i, (nint)ptr, len);
                        i += len;
                    }
                    return (uint)len;
                }
            };
            using var memory = new MemoryStream();
            var outputRef = new OutputStreamRef()
            {
                write = (byte* ptr, uint count) => {
                    if (count == 0)
                    {
                        return;
                    }
                    var temp = new byte[count];
                    Marshal.Copy((nint)ptr, temp, 0, temp.Length);
                    memory.Write(temp);
                }
            };
            var res = cb.Invoke(inputRef, outputRef);
            Expectation.ThrowIfNot(res == memory.Length);
            return memory.ToArray();
        }

        public static void Convert(Stream input, Stream output, Func<InputStreamRef, OutputStreamRef, long> cb)
        {
            var inputRef = new InputStreamRef()
            {
                read = (byte* ptr, uint count) => {
                    var buffer = new byte[count];
                    var len = input.Read(buffer, 0, buffer.Length);
                    if (len > 0)
                    {
                        Marshal.Copy(buffer, 0, (nint)ptr, len);
                    }
                    return (uint)len;
                }
            };
            var outputRef = new OutputStreamRef()
            {
                write = (byte* ptr, uint count) => {
                    if (count == 0)
                    {
                        return;
                    }
                    var temp = new byte[count];
                    Marshal.Copy((nint)ptr, temp, 0, temp.Length);
                    output.Write(temp);
                }
            };
            var res = cb.Invoke(inputRef, outputRef);
            Expectation.ThrowIfNot(res == output.Length);
        }
    }
}
