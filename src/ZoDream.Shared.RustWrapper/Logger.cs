using System.Runtime.InteropServices;

namespace ZoDream.Shared.RustWrapper
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct LoggerRef
    {
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public LoggerFn log;
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public LoggerProgressFn progress;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate void LoggerProgressFn(uint _1, uint _2, byte /*const*/ * _3);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate void LoggerFn(byte /*const*/ * _3);
}
