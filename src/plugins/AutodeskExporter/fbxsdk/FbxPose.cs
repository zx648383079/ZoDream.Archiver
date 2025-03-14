using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxPose : FbxObject
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxPose@fbxsdk@@SAPEAV12@PEAVFbxManager@2@PEBD@Z")]
        private static extern IntPtr CreateFromManager(IntPtr pManager, [MarshalAs(UnmanagedType.LPStr)] string pName);

        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxPose@fbxsdk@@SAPEAV12@PEAVFbxObject@2@PEBD@Z")]
        private static extern IntPtr CreateFromObject(IntPtr pObject, [MarshalAs(UnmanagedType.LPStr)] string pName);

        [DllImport(NativeMethods.DllName, EntryPoint = "?SetIsBindPose@FbxPose@fbxsdk@@QEAAX_N@Z")]
        private static extern void SetIsBindPoseInternal(IntPtr pHandle, bool pIsBindPose);

        [DllImport(NativeMethods.DllName, EntryPoint = "?Add@FbxPose@fbxsdk@@QEAAHPEAVFbxNode@2@AEBVFbxMatrix@2@_N2@Z")]
        private static extern int AddInternal(IntPtr pHandle, IntPtr pNode, IntPtr pMatrix, bool pLocalMatrix, bool pMultipleBindPose);

        public bool IsBindPose {
            get { unsafe { return *((char*)mType) == 'b'; } }
            set => SetIsBindPoseInternal(pHandle, value);
        }
        private IntPtr mType;

        public FbxPose(FbxManager manager, string name)
            : this(CreateFromManager(manager.Handle, name))
        {
        }

        public FbxPose(IntPtr InHandle)
            : base(InHandle)
        {
            mType = pHandle + 0x78;
        }

        public FbxPose(FbxObject obj, string name)
            : this (CreateFromObject(obj.Handle, name))
        {
        }

        public int Add(FbxNode node, FbxMatrix matrix, bool localMatrix = false, bool multipleBindPose = false)
        {
            return AddInternal(pHandle, node.Handle, matrix.Handle, localMatrix, multipleBindPose);
        }
    }

}
