using System.Runtime.InteropServices;

namespace ZoDream.Shared.RustWrapper
{
    [StructLayout(LayoutKind.Sequential, Size = 16)]
    public unsafe struct InputStreamRef
    {
        //public void* env_ptr;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public ReadCallFn read;
    }

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    //public unsafe /* static */ delegate uint ReadCallFn(void* _0, BufferRef input);
    public unsafe /* static */ delegate uint ReadCallFn(void* _0, byte * ptr, uint count);
}
