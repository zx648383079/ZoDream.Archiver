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
                        var input = new ByteArrayRef()
                        {
                            ptr = p,
                            len = (UIntPtr)buffer.Length,
                        };
                        var output = new ByteArrayRef()
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
