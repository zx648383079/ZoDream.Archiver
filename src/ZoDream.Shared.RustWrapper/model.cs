using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.Shared.RustWrapper
{
    [StructLayout(LayoutKind.Sequential, Size = 16)]
    public unsafe struct IntArrayRef
    {
        /// <summary>
        /// Pointer to the first element (if any).
        /// </summary>
        public int /*const*/ * ptr;

        /// <summary>
        /// Element count
        /// </summary>
        public UIntPtr len;
    }

    [StructLayout(LayoutKind.Sequential, Size = 16)]
    public unsafe struct ByteArrayRef
    {
        /// <summary>
        /// Pointer to the first element (if any).
        /// </summary>
        public byte /*const*/ * ptr;

        /// <summary>
        /// Element count
        /// </summary>
        public UIntPtr len;
    }

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    public unsafe delegate void LoggerFn(void* _0, int _1, byte /*const*/ * _2);

    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 16)]
    public unsafe struct LoggerRef
    {
        public void* env_ptr;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public LoggerFn call;
    }

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    public unsafe /* static */ delegate void VoidCallFn(
        void* _0,
        byte /*const*/ * _1);

    /// <summary>
    /// <c>&'lt mut (dyn 'lt + Send + FnMut(A1) -> Ret)</c>
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 16)]
    public unsafe struct FunCallRef
    {
        public void* env_ptr;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        public VoidCallFn call;
    }

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    public unsafe /* static */ delegate UInt16 RustCallFn(
        byte _0);
}
