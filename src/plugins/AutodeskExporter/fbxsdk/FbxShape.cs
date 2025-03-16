using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxShape : FbxGeometryBase
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxShape@fbxsdk@@SAPEAV12@PEAVFbxObject@2@PEBD@Z")]
        private static extern nint CreateFromObject(nint pObject, [MarshalAs(UnmanagedType.LPStr)] string pName);


        public FbxShape(nint InHandle)
            : base(InHandle)
        {
        }

        public FbxShape(FbxObject obj, string name)
            : this(CreateFromObject(obj.Handle, name))
        {
        }
    }
}
