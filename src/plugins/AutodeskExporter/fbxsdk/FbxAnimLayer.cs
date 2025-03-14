using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxAnimLayer : FbxCollection
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxAnimLayer@fbxsdk@@SAPEAV12@PEAVFbxObject@2@PEBD@Z")]
        private static extern IntPtr CreateFromObject(IntPtr pObject, [MarshalAs(UnmanagedType.LPStr)] string pName);
        public FbxAnimLayer(IntPtr InHandle)
            : base(InHandle)
        {
        }
        public FbxAnimLayer(FbxObject obj, string name)
            : this(CreateFromObject(obj.Handle, name))
        {
        }
    }
}
