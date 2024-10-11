using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ZoDream.Shared.RustWrapper
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct InputStreamRef
    {
        //public void* env_ptr;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        //[UnmanagedCallersOnly(CallConvs = new[] { typeof(ReadCallFn) })]
        public ReadCallFn read;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    //public unsafe /* static */ delegate uint ReadCallFn(void* _0, BufferRef input);
    public unsafe /* static */ delegate uint ReadCallFn(byte * ptr, uint count);
}
