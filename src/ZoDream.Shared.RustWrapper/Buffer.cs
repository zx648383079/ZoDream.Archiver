using System;
using System.Runtime.InteropServices;

namespace ZoDream.Shared.RustWrapper
{
    [StructLayout(LayoutKind.Sequential, Size = 16)]
    public unsafe struct BufferRef
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
}
