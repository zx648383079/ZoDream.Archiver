using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxStatus : FbxNative
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?GetErrorString@FbxStatus@fbxsdk@@QEBAPEBDXZ")]
        private static extern nint GetErrorStringInternal(nint handle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetCode@FbxStatus@fbxsdk@@QEBA?AW4EStatusCode@12@XZ")]
        private static extern EStatusCode GetCodeInternal(nint handle);

        public string ErrorString => FbxUtils.IntPtrToString(GetErrorStringInternal(Handle));
        public EStatusCode Code => GetCodeInternal(Handle);

        public FbxStatus(nint handle)
            : base(handle)
        {
        }
    }

}
