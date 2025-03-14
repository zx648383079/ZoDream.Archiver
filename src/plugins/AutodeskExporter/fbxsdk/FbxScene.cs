using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxScene : FbxDocument
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxScene@fbxsdk@@SAPEAV12@PEAVFbxManager@2@PEBD@Z")]
        private static extern IntPtr CreateFromManager(IntPtr pManager, [MarshalAs(UnmanagedType.LPStr)] string pName);

        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxScene@fbxsdk@@SAPEAV12@PEAVFbxObject@2@PEBD@Z")]
        private static extern IntPtr CreateFromObject(IntPtr pObject, [MarshalAs(UnmanagedType.LPStr)] string pName);

        [DllImport(NativeMethods.DllName, EntryPoint = "?SetSceneInfo@FbxScene@fbxsdk@@QEAAXPEAVFbxDocumentInfo@2@@Z")]
        private static extern void SetSceneInfoInternal(IntPtr InHandle, IntPtr pSceneInfo);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetGlobalSettings@FbxScene@fbxsdk@@QEBAAEBVFbxGlobalSettings@2@XZ")]
        private static extern IntPtr GetGlobalSettingsInternal(IntPtr InHandle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetRootNode@FbxScene@fbxsdk@@QEBAPEAVFbxNode@2@XZ")]
        private static extern IntPtr GetRootNodeInternal(IntPtr InHandle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetSceneInfo@FbxScene@fbxsdk@@QEAAPEAVFbxDocumentInfo@2@XZ")]
        private static extern IntPtr GetSceneInfoInternal(IntPtr handle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?AddPose@FbxScene@fbxsdk@@QEAA_NPEAVFbxPose@2@@Z")]
        private static extern bool AddPoseInternal(IntPtr pHandle, IntPtr pPose);

        public FbxGlobalSettings GlobalSettings => new FbxGlobalSettings(GetGlobalSettingsInternal(pHandle));
        public FbxNode RootNode => new FbxNode(GetRootNodeInternal(pHandle));

        public FbxDocumentInfo? SceneInfo {
            get {
                IntPtr p = GetSceneInfoInternal(pHandle);
                return (p != IntPtr.Zero) ? new FbxDocumentInfo(GetSceneInfoInternal(pHandle)) : null;
            }
            set {
                if (value is not null)
                {
                    SetSceneInfoInternal(pHandle, value.Handle);
                }
            }
        }

        public FbxScene(FbxManager Manager, string Name)
        {
            pHandle = CreateFromManager(Manager.Handle, Name);
        }

        public FbxScene(FbxObject Object, string Name)
        {
            pHandle = CreateFromObject(Object.Handle, Name);
        }

        public bool AddPose(FbxPose pose)
        {
            return AddPoseInternal(pHandle, pose.Handle);
        }
    }
}
