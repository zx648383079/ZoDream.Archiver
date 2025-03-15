using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxManager : FbxNative
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxManager@fbxsdk@@SAPEAV12@XZ")]
        private static extern IntPtr CreateInternal();

        [DllImport(NativeMethods.DllName, EntryPoint = "?Destroy@FbxManager@fbxsdk@@UEAAXXZ")]
        private static extern IntPtr DestroyInternal(IntPtr InHandle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?SetIOSettings@FbxManager@fbxsdk@@UEAAXPEAVFbxIOSettings@2@@Z")]
        private static extern void SetIOSettingsInternal(IntPtr handle, IntPtr pIOSettings);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetIOSettings@FbxManager@fbxsdk@@UEBAPEAVFbxIOSettings@2@XZ")]
        private static extern IntPtr GetIOSettingsInternal(IntPtr handle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetVersion@FbxManager@fbxsdk@@SAPEBD_N@Z", CharSet = CharSet.Unicode)]
        private static extern IntPtr GetVersionInternal(IntPtr InHandle, bool pFull);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetFileFormatVersion@FbxManager@fbxsdk@@SAXAEAH00@Z")]
        private static extern void GetFileFormatVersionInternal(ref int pMajor, ref int pMinor, ref int pRevision);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetIOPluginRegistry@FbxManager@fbxsdk@@QEBAPEAVFbxIOPluginRegistry@2@XZ")]
        private static extern IntPtr GetIOPluginRegistryInternal(IntPtr InHandle);

        public static void GetFileFormatVersion(out int pMajor, out int pMinor, out int pRevision)
        {
            pMajor = 0; pMinor = 0; pRevision = 0;
            GetFileFormatVersionInternal(ref pMajor, ref pMinor, ref pRevision);
        }

        public FbxManager()
        {
            _leaveFree = true;
            NativeMethods.Ready();
            Handle = CreateInternal();
        }


        public FbxIOPluginRegistry IOPluginRegistry => new FbxIOPluginRegistry(GetIOPluginRegistryInternal(Handle));

        public void SetIOSettings(FbxIOSettings settings)
        {
            SetIOSettingsInternal(Handle, settings.Handle);
        }

        public FbxIOSettings GetIOSettings()
        {
            return new FbxIOSettings(GetIOSettingsInternal(Handle));
        }

        public string GetVersion(bool pFull = true)
        {
            IntPtr StrPtr = GetVersionInternal(Handle, pFull);
            return FbxUtils.IntPtrToString(StrPtr);
        }


        protected override void Dispose(bool bDisposing)
        {
            DestroyInternal(Handle);
            base.Dispose(bDisposing);
        }
    }

}
