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
        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxFileTexture@fbxsdk@@SAPEAV12@PEAVFbxManager@2@PEBD@Z")]
        private static extern nint CreateFromManager(nint handle, [MarshalAs(UnmanagedType.LPStr)] string name);
        [DllImport(NativeMethods.DllName, EntryPoint = "?SetMaterialUse@FbxFileTexture@fbxsdk@@QEAAXW4EMaterialUse@12@@Z")]
        private static extern void SetMaterialUseInternal(nint pHandle, EMaterialUse pMaterialUse);
        [DllImport(NativeMethods.DllName, EntryPoint = "?GetMaterialUse@FbxFileTexture@fbxsdk@@QEBA?AW4EMaterialUse@12@XZ")]
        private static extern EMaterialUse GetMaterialUseInternal(nint pHandle);
        [DllImport(NativeMethods.DllName, EntryPoint = "?SetFileName@FbxFileTexture@fbxsdk@@QEAA_NPEBD@Z")]
        private static extern void SetFileNameInternal(nint pHandle, [MarshalAs(UnmanagedType.LPStr)] string name);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetFileName@FbxFileTexture@fbxsdk@@QEBAPEBDXZ")]
        private static extern string GetFileNameInternal(nint pHandle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?SetRelativeFileName@FbxFileTexture@fbxsdk@@QEAA_NPEBD@Z")]
        private static extern void SetRelativeFileNameInternal(nint pHandle, [MarshalAs(UnmanagedType.LPStr)] string name);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetRelativeFileName@FbxFileTexture@fbxsdk@@QEBAPEBDXZ")]
        private static extern string GetRelativeFileNameInternal(nint pHandle);
        [DllImport(NativeMethods.DllName, EntryPoint = "?SetMediaName@FbxFileTexture@fbxsdk@@QEAAXPEBD@Z")]
        private static extern void SetMediaNameInternal(nint pHandle, [MarshalAs(UnmanagedType.LPStr)] string name);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetMediaName@FbxFileTexture@fbxsdk@@QEAAAEAVFbxString@2@XZ")]
        private static extern string GetMediaNameInternal(nint pHandle);
        public FbxFileTexture(FbxManager manager, string name)
           : base(CreateFromManager(manager.Handle, name))
        {
        }

        public FbxFileTexture(FbxObject obj, string name)
            : base(CreateFromObject(obj.Handle, name))
        {
        }


        public FbxFileTexture(nint handle)
            : base(handle)
        {
        }


        public string FileName {
            get => GetFileNameInternal(Handle);
            set => SetFileNameInternal(Handle, value);
        }

        public string MediaName {
            get => GetMediaNameInternal(Handle);
            set => SetMediaNameInternal(Handle, value);
        }

        public string RelativeFileName {
            get => GetRelativeFileNameInternal(Handle);
            set => SetRelativeFileNameInternal(Handle, value);
        }

        public EMaterialUse MaterialUse {
            get => GetMaterialUseInternal(Handle);
            set => SetMaterialUseInternal(Handle, value);
        }

    
    }

}
