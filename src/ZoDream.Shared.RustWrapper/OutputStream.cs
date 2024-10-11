using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.Shared.RustWrapper
{
    [StructLayout(LayoutKind.Sequential, Size = 16)]
    public unsafe struct OutputStreamRef
    {
        //public void* env_ptr;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public WriteCallFn write;
    }

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    //public unsafe delegate void WriteCallFn(void* _0, BufferRef input);
    public unsafe delegate void WriteCallFn(void* _0, byte* ptr, uint count);
}
