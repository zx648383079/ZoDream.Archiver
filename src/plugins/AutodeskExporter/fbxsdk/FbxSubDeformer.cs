using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxSubDeformer : FbxObject
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxSubDeformer@fbxsdk@@SAPEAV12@PEAVFbxManager@2@PEBD@Z")]
        private static extern nint CreateFromManager(nint pManager, [MarshalAs(UnmanagedType.LPStr)] string pName);

        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxSubDeformer@fbxsdk@@SAPEAV12@PEAVFbxObject@2@PEBD@Z")]
        private static extern nint CreateFromObject(nint pObject, [MarshalAs(UnmanagedType.LPStr)] string pName);

        internal FbxSubDeformer()
        {
        }

        public FbxSubDeformer(FbxManager manager, string name)
            : base(CreateFromManager(manager.Handle, name))
        {
        }

        public FbxSubDeformer(nint InHandle)
            : base(InHandle)
        {
        }

        public FbxSubDeformer(FbxObject obj, string name)
            : base(CreateFromObject(obj.Handle, name))
        {
        }
    }

}
