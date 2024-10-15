using System.Runtime.InteropServices;

namespace ZoDream.Shared.RustWrapper
{
    internal unsafe static partial class NativeMethods
    {
        

        [DllImport(RustDllName, ExactSpelling = true)]
        public static unsafe extern CompressorRef* find_compressor(CompressionID compression);

        [DllImport(RustDllName, ExactSpelling = true)]
        public static unsafe extern long compress_compressor(CompressorRef* compressor, ref InputStreamRef input, ref OutputStreamRef output, ref LoggerRef logger);

        [DllImport(RustDllName, ExactSpelling = true)]
        public static unsafe extern long decompress_compressor(CompressorRef* compressor, ref InputStreamRef input, ref OutputStreamRef output, ref LoggerRef logger);


        [DllImport(RustDllName, ExactSpelling = true)]
        public static unsafe extern void free_compressor(CompressorRef* compressor);
    }
}
