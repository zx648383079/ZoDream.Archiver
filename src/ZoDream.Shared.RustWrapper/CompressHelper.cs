using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.Shared.RustWrapper
{
    public static class CompressHelper
    {

        public static void StringToPtr(string s)
        {
            var ptr = Marshal.StringToCoTaskMemUTF8(s);
            // var res = (byte*)ptr;
            Marshal.FreeCoTaskMem(ptr);
        }

        public static byte[] Lz4Decompress(byte[] buffer, int uncompressLength)
        {
            var res = new byte[uncompressLength];
            var resLen = 0;
            unsafe
            {
                fixed(byte* p = buffer)
                {
                    fixed(byte* t = res)
                    {
                        var input = new BufferRef()
                        {
                            ptr = p,
                            len = (UIntPtr)buffer.Length,
                        };
                        var output = new BufferRef()
                        {
                            ptr = t,
                            len = (UIntPtr)res.Length,
                        };
                        resLen = NativeMethods.lz4_decompress(input, output, new LoggerRef()
                        {
                            env_ptr = (void*)0xbad00,
                            call = (void* _, int p, byte * msgPtr) => {
                                var msg = Marshal.PtrToStringUTF8((IntPtr)msgPtr);
                                Debug.WriteLine($"logger call: {p}; {msg} ");
                            },
                        });
                    }
                }
            }
            Debug.WriteLine($"lz4: {resLen}, {uncompressLength}");
            return res;
        }
    }
}
