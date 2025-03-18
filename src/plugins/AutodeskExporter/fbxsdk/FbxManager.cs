using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxManager : FbxNative
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxManager@fbxsdk@@SAPEAV12@XZ")]
        private static extern nint CreateInternal();

        [DllImport(NativeMethods.DllName, EntryPoint = "?Destroy@FbxManager@fbxsdk@@UEAAXXZ")]
        private static extern nint DestroyInternal(nint InHandle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?SetIOSettings@FbxManager@fbxsdk@@UEAAXPEAVFbxIOSettings@2@@Z")]
        private static extern void SetIOSettingsInternal(nint handle, nint pIOSettings);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetIOSettings@FbxManager@fbxsdk@@UEBAPEAVFbxIOSettings@2@XZ")]
        private static extern nint GetIOSettingsInternal(nint handle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetVersion@FbxManager@fbxsdk@@SAPEBD_N@Z", CharSet = CharSet.Unicode)]
        private static extern nint GetVersionInternal(nint InHandle, bool pFull);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetFileFormatVersion@FbxManager@fbxsdk@@SAXAEAH00@Z")]
        private static extern void GetFileFormatVersionInternal(ref int pMajor, ref int pMinor, ref int pRevision);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetIOPluginRegistry@FbxManager@fbxsdk@@QEBAPEAVFbxIOPluginRegistry@2@XZ")]
        private static extern nint GetIOPluginRegistryInternal(nint InHandle);

        public static void GetFileFormatVersion(out int pMajor, out int pMinor, out int pRevision)
        {
            pMajor = 0; pMinor = 0; pRevision = 0;
            GetFileFormatVersionInternal(ref pMajor, ref pMinor, ref pRevision);
        }

        public FbxManager()
        {
            NativeMethods.Ready();
            Handle = CreateInternal();
            _leaveFree = true;
        }

        public FbxIOSettings IOSettings { get => new(GetIOSettingsInternal(Handle)); set => SetIOSettingsInternal(Handle, value.Handle); }

        public FbxIOPluginRegistry IOPluginRegistry => new(GetIOPluginRegistryInternal(Handle));


        public string GetVersion(bool pFull = true)
        {
            nint StrPtr = GetVersionInternal(Handle, pFull);
            return FbxUtils.IntPtrToString(StrPtr);
        }


        protected override void Dispose(bool bDisposing)
        {
            if (bDisposing)
            {
                DestroyInternal(Handle);
            }
            // base.Dispose(bDisposing);
        }
    }

}
