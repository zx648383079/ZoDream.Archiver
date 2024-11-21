using System.Runtime.InteropServices;

namespace ZoDream.Shared.RustWrapper
{
    internal unsafe static partial class NativeMethods
    {
        

        [DllImport(RustDllName, ExactSpelling = true)]
        public static unsafe extern PainterRef* find_painter(PixelID pixel, uint width, uint height, uint bw, uint bh);

        [DllImport(RustDllName, ExactSpelling = true)]
        public static unsafe extern long encode_painter(PainterRef* compressor, ref InputStreamRef input, ref OutputStreamRef output, ref LoggerRef logger);

        [DllImport(RustDllName, ExactSpelling = true)]
        public static unsafe extern long decode_painter(PainterRef* compressor, ref InputStreamRef input, ref OutputStreamRef output, ref LoggerRef logger);


        [DllImport(RustDllName, ExactSpelling = true)]
        public static unsafe extern void free_painter(PainterRef* compressor);
    }
}
