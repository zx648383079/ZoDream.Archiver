using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxStatus : FbxNative
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?GetErrorString@FbxStatus@fbxsdk@@QEBAPEBDXZ")]
        private static extern IntPtr GetErrorStringInternal(IntPtr handle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetCode@FbxStatus@fbxsdk@@QEBA?AW4EStatusCode@12@XZ")]
        private static extern EStatusCode GetCodeInternal(IntPtr handle);

        public string ErrorString => FbxUtils.IntPtrToString(GetErrorStringInternal(pHandle));
        public EStatusCode Code => GetCodeInternal(pHandle);

        public FbxStatus(IntPtr handle)
        {
            pHandle = handle;
        }
    }

}
