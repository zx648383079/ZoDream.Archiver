using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxAnimStack : FbxCollection
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxAnimStack@fbxsdk@@SAPEAV12@PEAVFbxObject@2@PEBD@Z")]
        private static extern IntPtr CreateFromObject(IntPtr pObject, [MarshalAs(UnmanagedType.LPStr)] string pName);


        public FbxAnimStack(IntPtr InHandle)
            : base(InHandle)
        {
        }
        public FbxAnimStack(FbxObject obj, string name)
            : this(CreateFromObject(obj.Handle, name))
        {
        }
    }
}
