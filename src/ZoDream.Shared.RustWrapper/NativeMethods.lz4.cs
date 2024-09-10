using System.Runtime.InteropServices;

namespace ZoDream.Shared.RustWrapper
{
    internal unsafe static partial class NativeMethods
    {
        

        [DllImport(__DllName, ExactSpelling = true)]
        public static unsafe extern int lz4_decompress(ByteArrayRef input, ByteArrayRef output, LoggerRef logger);



    }
}
