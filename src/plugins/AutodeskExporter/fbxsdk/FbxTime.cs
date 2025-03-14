using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxTime : FbxNative
    {
        public static FbxTime FBXSDK_TIME_INFINITE = new(0x7fffffffffffffff);

        [DllImport(NativeMethods.DllName, EntryPoint = "??0FbxTime@fbxsdk@@QEAA@_J@Z")]
        private static extern void ConstructInternal(IntPtr inHandle, long pTime);

        public FbxTime(long time)
        {
            pHandle = FbxUtils.FbxMalloc(8);
            ConstructInternal(pHandle, time);
            bNeedsFreeing = true;
        }

        public FbxTime(float time)
            : this((long)(time * 1000))
        {

        }
    }

}
