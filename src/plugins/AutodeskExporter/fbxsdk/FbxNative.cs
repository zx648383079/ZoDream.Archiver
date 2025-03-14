using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxNative
    {
        protected IntPtr pHandle;
        protected IntPtr vTable;

        internal IntPtr Handle => pHandle;
        protected bool bNeedsFreeing;

        public FbxNative() { }
        public FbxNative(IntPtr InHandle)
        {
            pHandle = InHandle;
            vTable = Marshal.ReadIntPtr(pHandle, 0);
        }
        ~FbxNative()
        {
            if (bNeedsFreeing)
                FbxUtils.FbxFree(pHandle);
        }
    }
}
