using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxFileTexture : FbxTexture
    {

        public enum EMaterialUse
        {
            eModelMaterial,     //! Texture uses model material.
            eDefaultMaterial    //! Texture does not use model material.
        };

        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxFileTexture@fbxsdk@@SAPEAV12@PEAVFbxObject@2@PEBD@Z")]
        private static extern nint CreateFromObject(nint handle, [MarshalAs(UnmanagedType.LPStr)] string name);
        [DllImport(NativeMethods.DllName, EntryPoint = "?SetMaterialUse@FbxFileTexture@fbxsdk@@QEAAXW4EMaterialUse@12@@Z")]
        private static extern void SetMaterialUseInternal(nint pHandle, EMaterialUse pMaterialUse);
        [DllImport(NativeMethods.DllName, EntryPoint = "?SetFileName@FbxFileTexture@fbxsdk@@QEAA_NPEBD@Z")]
        private static extern void SetFileNameInternal(nint pHandle, [MarshalAs(UnmanagedType.LPStr)] string name);
        

        public FbxFileTexture(FbxObject obj, string name)
            : base(CreateFromObject(obj.Handle, name))
        {
        }


        public FbxFileTexture(IntPtr handle)
            : base(handle)
        {
        }


        internal void SetFileName(string name)
        {
            SetFileNameInternal(pHandle, name);
        }

        
        internal void SetMaterialUse(EMaterialUse pMaterialUse)
        {
            SetMaterialUseInternal(pHandle, pMaterialUse);
        }

    
    }

}
