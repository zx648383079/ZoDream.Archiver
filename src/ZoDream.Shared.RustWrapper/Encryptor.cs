using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace ZoDream.Shared.RustWrapper
{
    public unsafe class Encryptor: IDisposable
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

        private readonly EncryptorRef* _instance;

        public byte[] Encrypt(byte[] buffer)
        {
            return Convert(buffer, (inputRef, outputRef) => {
                return NativeMethods.encrypt_encryptor(_instance, &inputRef, &outputRef);
            });
        }

        public void Encrypt(Stream input, Stream output)
        {
            Convert(input, output, (inputRef, outputRef) => {
                return NativeMethods.encrypt_encryptor(_instance, &inputRef, &outputRef);
            });
        }

        public byte[] Decrypt(byte[] buffer)
        {
            return Convert(buffer, (inputRef, outputRef) => {
                return NativeMethods.decrypt_encryptor(_instance, &inputRef, &outputRef);
            });
        }
        public void Decrypt(Stream input, Stream output)
        {
            Convert(input, output, (inputRef, outputRef) => {
                return NativeMethods.decrypt_encryptor(_instance, &inputRef, &outputRef);
            });
        }

        public void Dispose()
        {
            NativeMethods.free_encryptor(_instance);
        }


        public static byte[] Convert(byte[] buffer, Func<InputStreamRef, OutputStreamRef, int> cb)
        {
            var i = 0;
            var inputRef = new InputStreamRef()
            {
                read = (void* _, BufferRef data) => {
                    var len = Math.Min(buffer.Length - i, (int)data.len);
                    if (len > 0)
                    {
                        Marshal.Copy(buffer, i, (nint)data.ptr, len);
                        i += len;
                    }
                    return (uint)len;
                }
            };
            using var memory = new MemoryStream();
            var outputRef = new OutputStreamRef()
            {
                write = (void* _0, BufferRef data) => {
                    if (data.len == 0)
                    {
                        return;
                    }
                    var temp = new byte[data.len];
                    Marshal.Copy((nint)data.ptr, temp, 0, temp.Length);
                    memory.Write(temp);
                }
            };
            var res = cb.Invoke(inputRef, outputRef);
            Debug.Assert(res == memory.Length);
            return memory.ToArray();
        }

        public static void Convert(Stream input, Stream output, Func<InputStreamRef, OutputStreamRef, int> cb)
        {
            var inputRef = new InputStreamRef()
            {
                read = (void* _, BufferRef data) => {
                    var buffer = new byte[data.len];
                    var len = input.Read(buffer, 0, buffer.Length);
                    if (len > 0)
                    {
                        Marshal.Copy(buffer, 0, (nint)data.ptr, len);
                    }
                    return (uint)len;
                }
            };
            var outputRef = new OutputStreamRef()
            {
                write = (void* _0, BufferRef data) => {
                    if (data.len == 0)
                    {
                        return;
                    }
                    var temp = new byte[data.len];
                    Marshal.Copy((nint)data.ptr, temp, 0, temp.Length);
                    output.Write(temp);
                }
            };
            var res = cb.Invoke(inputRef, outputRef);
            Debug.Assert(res == output.Length);
        }
    }
}
