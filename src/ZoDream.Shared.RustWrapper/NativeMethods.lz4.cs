using System.Runtime.InteropServices;

namespace ZoDream.Shared.RustWrapper
{
    internal unsafe static partial class NativeMethods
    {
        

        [DllImport(RustDllName, ExactSpelling = true)]
        public static unsafe extern int lz4_decompress(BufferRef input, BufferRef output, LoggerRef logger);



    }
}
