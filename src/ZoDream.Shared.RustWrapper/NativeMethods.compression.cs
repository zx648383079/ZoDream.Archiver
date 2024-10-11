using System.Runtime.InteropServices;

namespace ZoDream.Shared.RustWrapper
{
    internal unsafe static partial class NativeMethods
    {
        

        [DllImport(RustDllName, ExactSpelling = true)]
        public static unsafe extern CompressorRef* find_compressor(CompressionID compression);

        [DllImport(RustDllName, ExactSpelling = true)]
        public static unsafe extern int compress_compressor(CompressorRef* compressor, InputStreamRef* input, OutputStreamRef* output);

        [DllImport(RustDllName, ExactSpelling = true)]
        public static unsafe extern int decompress_compressor(CompressorRef* compressor, InputStreamRef* input, OutputStreamRef* output);


        [DllImport(RustDllName, ExactSpelling = true)]
        public static unsafe extern void free_compressor(CompressorRef* compressor);
    }
}
