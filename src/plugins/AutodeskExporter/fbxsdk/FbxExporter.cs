using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxExporter : FbxObject
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxExporter@fbxsdk@@SAPEAV12@PEAVFbxManager@2@PEBD@Z")]
        private static extern nint CreateFromManager(nint pManager, [MarshalAs(UnmanagedType.LPStr)] string pName);

        [DllImport(NativeMethods.DllName, EntryPoint = "?Initialize@FbxExporter@fbxsdk@@UEAA_NPEBDHPEAVFbxIOSettings@2@@Z", CallingConvention = CallingConvention.ThisCall)]
        private static extern bool InitializeInternal(nint InHandle, [MarshalAs(UnmanagedType.LPStr)] string pFileName, int pFileFormat, nint pIOSettings);

        [DllImport(NativeMethods.DllName, EntryPoint = "?Export@FbxExporter@fbxsdk@@QEAA_NPEAVFbxDocument@2@_N@Z", CallingConvention = CallingConvention.ThisCall)]
        private static extern bool ExportInternal(nint InHandle, nint pDocument, bool pNonBlocking);

        [DllImport(NativeMethods.DllName, EntryPoint = "?SetFileExportVersion@FbxExporter@fbxsdk@@QEAA_NVFbxString@2@W4ERenamingMode@FbxSceneRenamer@2@@Z", CallingConvention = CallingConvention.ThisCall)]
        private static extern bool SetFileExportVersionInternal(nint InHandle, nint pVersion, int pRenamingMode);

        public FbxExporter(FbxManager Manager, string Name)
            : base(CreateFromManager(Manager.Handle, Name))
        {
        }

        public bool Initialize(string pFileName, int pFileFormat = -1, FbxIOSettings? pIOSettings = null)
        {
            var ptr = pIOSettings is not null ? pIOSettings.Handle : nint.Zero;
            return InitializeInternal(Handle, pFileName, pFileFormat, ptr);
        }

        public bool Export(FbxDocument pDocument, bool pNonBlocking = false)
        {
            return ExportInternal(Handle, pDocument.Handle, pNonBlocking);
        }

        public bool SetFileExportVersion(FbxVersion version)
        {
            var ptr = FbxString.Construct(Enum.GetName(version) ?? "FBX202000");
            return SetFileExportVersionInternal(Handle, ptr, 0);
        }
    }

}
