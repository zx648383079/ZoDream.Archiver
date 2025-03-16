using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxAnimLayer : FbxCollection
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxAnimLayer@fbxsdk@@SAPEAV12@PEAVFbxObject@2@PEBD@Z")]
        private static extern nint CreateFromObject(nint pObject, [MarshalAs(UnmanagedType.LPStr)] string pName);
        public FbxAnimLayer(nint InHandle)
            : base(InHandle)
        {
        }
        public FbxAnimLayer(FbxObject obj, string name)
            : this(CreateFromObject(obj.Handle, name))
        {
        }
    }
}
