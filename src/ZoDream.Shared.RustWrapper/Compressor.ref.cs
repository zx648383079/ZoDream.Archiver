using System.Runtime.InteropServices;

namespace ZoDream.Shared.RustWrapper
{
    [StructLayout(LayoutKind.Sequential, Size = 16)]
    public unsafe struct CompressorRef
    {
        //public void* env_ptr;

        //[MarshalAs(UnmanagedType.FunctionPtr)]
        //public ConvertCallFn compress;

        //[MarshalAs(UnmanagedType.FunctionPtr)]
        //public ConvertCallFn decompress;
    }

}
