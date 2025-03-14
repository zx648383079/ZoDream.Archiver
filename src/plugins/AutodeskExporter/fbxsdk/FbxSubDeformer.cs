using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxSubDeformer : FbxObject
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxSubDeformer@fbxsdk@@SAPEAV12@PEAVFbxManager@2@PEBD@Z")]
        private static extern IntPtr CreateFromManager(IntPtr pManager, [MarshalAs(UnmanagedType.LPStr)] string pName);

        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxSubDeformer@fbxsdk@@SAPEAV12@PEAVFbxObject@2@PEBD@Z")]
        private static extern IntPtr CreateFromObject(IntPtr pObject, [MarshalAs(UnmanagedType.LPStr)] string pName);

        internal FbxSubDeformer()
        {
        }

        public FbxSubDeformer(FbxManager manager, string name)
        {
            pHandle = CreateFromManager(manager.Handle, name);
        }

        public FbxSubDeformer(IntPtr InHandle)
            : base(InHandle)
        {
        }

        public FbxSubDeformer(FbxObject obj, string name)
        {
            pHandle = CreateFromObject(obj.Handle, name);
        }
    }

}
