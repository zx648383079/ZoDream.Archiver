using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxExporter : FbxObject
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxExporter@fbxsdk@@SAPEAV12@PEAVFbxManager@2@PEBD@Z")]
        private static extern IntPtr CreateFromManager(IntPtr pManager, [MarshalAs(UnmanagedType.LPStr)] string pName);

        [DllImport(NativeMethods.DllName, EntryPoint = "?Initialize@FbxExporter@fbxsdk@@UEAA_NPEBDHPEAVFbxIOSettings@2@@Z", CallingConvention = CallingConvention.ThisCall)]
        private static extern bool InitializeInternal(IntPtr InHandle, [MarshalAs(UnmanagedType.LPStr)] string pFileName, int pFileFormat, IntPtr pIOSettings);

        [DllImport(NativeMethods.DllName, EntryPoint = "?Export@FbxExporter@fbxsdk@@QEAA_NPEAVFbxDocument@2@_N@Z", CallingConvention = CallingConvention.ThisCall)]
        private static extern bool ExportInternal(IntPtr InHandle, IntPtr pDocument, bool pNonBlocking);

        [DllImport(NativeMethods.DllName, EntryPoint = "?SetFileExportVersion@FbxExporter@fbxsdk@@QEAA_NVFbxString@2@W4ERenamingMode@FbxSceneRenamer@2@@Z", CallingConvention = CallingConvention.ThisCall)]
        private static extern bool SetFileExportVersionInternal(IntPtr InHandle, IntPtr pVersion, int pRenamingMode);

        public FbxExporter(FbxManager Manager, string Name)
        {
            pHandle = CreateFromManager(Manager.Handle, Name);
        }

        public bool Initialize(string pFileName, int pFileFormat = -1, FbxIOSettings? pIOSettings = null)
        {
            IntPtr Ptr = pIOSettings is not null ? pIOSettings.Handle : IntPtr.Zero;
            return InitializeInternal(pHandle, pFileName, pFileFormat, Ptr);
        }

        public bool Export(FbxDocument pDocument, bool pNonBlocking = false)
        {
            return ExportInternal(pHandle, pDocument.Handle, pNonBlocking);
        }

        public bool SetFileExportVersion(string pVersion)
        {
            IntPtr Ptr = FbxString.Construct(pVersion);
            return SetFileExportVersionInternal(pHandle, Ptr, 0);
        }
    }

}
