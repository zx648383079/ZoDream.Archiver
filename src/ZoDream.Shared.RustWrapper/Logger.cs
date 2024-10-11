using System.Runtime.InteropServices;

namespace ZoDream.Shared.RustWrapper
{
    [StructLayout(LayoutKind.Sequential, Size = 16)]
    public unsafe struct LoggerRef
    {
        public void* env_ptr;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public LoggerProgressFn call;
    }

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    public unsafe delegate void LoggerProgressFn(void* _0, int _1, byte /*const*/ * _2);
}
