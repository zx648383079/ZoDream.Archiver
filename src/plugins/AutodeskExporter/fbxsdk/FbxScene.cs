using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxScene : FbxDocument
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxScene@fbxsdk@@SAPEAV12@PEAVFbxManager@2@PEBD@Z")]
        private static extern nint CreateFromManager(nint pManager, [MarshalAs(UnmanagedType.LPStr)] string pName);

        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxScene@fbxsdk@@SAPEAV12@PEAVFbxObject@2@PEBD@Z")]
        private static extern nint CreateFromObject(nint pObject, [MarshalAs(UnmanagedType.LPStr)] string pName);

        [DllImport(NativeMethods.DllName, EntryPoint = "?SetSceneInfo@FbxScene@fbxsdk@@QEAAXPEAVFbxDocumentInfo@2@@Z")]
        private static extern void SetSceneInfoInternal(nint InHandle, nint pSceneInfo);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetGlobalSettings@FbxScene@fbxsdk@@QEBAAEBVFbxGlobalSettings@2@XZ")]
        private static extern nint GetGlobalSettingsInternal(nint InHandle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetRootNode@FbxScene@fbxsdk@@QEBAPEAVFbxNode@2@XZ")]
        private static extern nint GetRootNodeInternal(nint InHandle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetSceneInfo@FbxScene@fbxsdk@@QEAAPEAVFbxDocumentInfo@2@XZ")]
        private static extern nint GetSceneInfoInternal(nint handle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?AddPose@FbxScene@fbxsdk@@QEAA_NPEAVFbxPose@2@@Z")]
        private static extern bool AddPoseInternal(nint pHandle, nint pPose);

        public FbxGlobalSettings GlobalSettings => new FbxGlobalSettings(GetGlobalSettingsInternal(Handle));
        public FbxNode RootNode => new FbxNode(GetRootNodeInternal(Handle));

        public FbxDocumentInfo? SceneInfo {
            get {
                nint p = GetSceneInfoInternal(Handle);
                return (p != nint.Zero) ? new FbxDocumentInfo(p) : null;
            }
            set {
                if (value is not null)
                {
                    SetSceneInfoInternal(Handle, value.Handle);
                }
            }
        }

        public FbxScene(FbxManager Manager, string Name)
            : base(CreateFromManager(Manager.Handle, Name))
        {
        }

        public FbxScene(FbxObject Object, string Name)
            : base(CreateFromObject(Object.Handle, Name))
        {
        }

        public bool AddPose(FbxPose pose)
        {
            return AddPoseInternal(Handle, pose.Handle);
        }

    }
}
