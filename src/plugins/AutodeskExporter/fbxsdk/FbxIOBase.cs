using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxIOBase : FbxObject
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?GetStatus@FbxIOBase@fbxsdk@@QEAAAEAVFbxStatus@2@XZ")]
        private static extern IntPtr GetStatusInternal(IntPtr handle);

        public FbxStatus Status => new FbxStatus(GetStatusInternal(Handle));

        public FbxIOBase(nint handle)
            : base(handle)
        {
            
        }
    }
}
