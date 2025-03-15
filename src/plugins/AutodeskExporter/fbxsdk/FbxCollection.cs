using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxCollection : FbxObject
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?AddMember@FbxCollection@fbxsdk@@UEAA_NPEAVFbxObject@2@@Z")]
        private static extern void AddMemberInternal(IntPtr pObject, nint layer);

        public FbxCollection() { }
        public FbxCollection(IntPtr InHandle)
            : base(InHandle)
        {
        }

        internal void AddMember(FbxObject obj)
        {
            AddMemberInternal(Handle, obj.Handle);
        }
    }
}
