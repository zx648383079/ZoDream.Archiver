using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.Shared.RustWrapper
{
    [StructLayout(LayoutKind.Sequential, Size = 16)]
    public unsafe struct EncryptorRef
    {
        //public void* env_ptr;

        //[MarshalAs(UnmanagedType.FunctionPtr)]
        //public ConvertCallFn encrypt;

        //[MarshalAs(UnmanagedType.FunctionPtr)]
        //public ConvertCallFn decrypt;
    }

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    public unsafe /* static */ delegate void ConvertCallFn(
        void* _0,
        BufferRef input, OutputStreamRef output);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    public unsafe /* static */ delegate void ConvertStreamFn(
        void* _0,
        InputStreamRef input, OutputStreamRef output);
}
