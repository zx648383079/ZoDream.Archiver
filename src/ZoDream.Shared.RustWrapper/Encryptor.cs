using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
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
                return NativeMethods.encrypt_encryptor(_instance, ref inputRef, ref outputRef, new LoggerRef()
                {
                    env_ptr = (void*)0xbad00,
                    call = (void* _, int p, byte* msgPtr) => {
                        var msg = Marshal.PtrToStringUTF8((IntPtr)msgPtr);
                        Debug.WriteLine($"logger call: {p}; {msg} ");
                    },
                });
            });
        }

        public void Encrypt(Stream input, Stream output)
        {
            Convert(input, output, (inputRef, outputRef) => {
                return NativeMethods.encrypt_encryptor(_instance, ref inputRef, ref outputRef, new LoggerRef()
                {
                    env_ptr = (void*)0xbad00,
                    call = (void* _, int p, byte* msgPtr) => {
                        var msg = Marshal.PtrToStringUTF8((IntPtr)msgPtr);
                        Debug.WriteLine($"logger call: {p}; {msg} ");
                    },
                });
            });
        }

        public byte[] Decrypt(byte[] buffer)
        {
            return Convert(buffer, (inputRef, outputRef) => {
                return NativeMethods.decrypt_encryptor(_instance, ref inputRef, ref outputRef);
            });
        }
        public void Decrypt(Stream input, Stream output)
        {
            Convert(input, output, (inputRef, outputRef) => {
                return NativeMethods.decrypt_encryptor(_instance, ref inputRef, ref outputRef);
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
                read = (byte* ptr, uint count) => {
                    var len = Math.Min(buffer.Length - i, (int)count);
                    if (len > 0)
                    {
                        Debug.WriteLine(count);
                        Marshal.Copy(buffer, i, (nint)ptr, len);
                        Debug.WriteLine(3);
                        i += len;
                    }
                    return (uint)len;
                }
            };
            using var memory = new MemoryStream();
            var outputRef = new OutputStreamRef()
            {
                write = (byte* ptr, uint count) => {
                    Debug.WriteLine(4);
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
            Debug.Assert(res == memory.Length);
            return memory.ToArray();
        }

        public static void Convert(Stream input, Stream output, Func<InputStreamRef, OutputStreamRef, int> cb)
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
            Debug.Assert(res == output.Length);
        }
    }
}
