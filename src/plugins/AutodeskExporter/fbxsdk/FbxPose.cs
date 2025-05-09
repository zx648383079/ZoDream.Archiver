﻿using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxPose : FbxObject
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxPose@fbxsdk@@SAPEAV12@PEAVFbxManager@2@PEBD@Z")]
        private static extern nint CreateFromManager(nint pManager, [MarshalAs(UnmanagedType.LPStr)] string pName);

        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxPose@fbxsdk@@SAPEAV12@PEAVFbxObject@2@PEBD@Z")]
        private static extern nint CreateFromObject(nint pObject, [MarshalAs(UnmanagedType.LPStr)] string pName);

        [DllImport(NativeMethods.DllName, EntryPoint = "?SetIsBindPose@FbxPose@fbxsdk@@QEAAX_N@Z")]
        private static extern void SetIsBindPoseInternal(nint pHandle, bool pIsBindPose);

        [DllImport(NativeMethods.DllName, EntryPoint = "?Add@FbxPose@fbxsdk@@QEAAHPEAVFbxNode@2@AEBVFbxMatrix@2@_N2@Z")]
        private static extern int AddInternal(nint pHandle, nint pNode, nint pMatrix, bool pLocalMatrix, bool pMultipleBindPose);

        public bool IsBindPose {
            get { unsafe { return *((char*)_isBindPose) == 'b'; } }
            set => SetIsBindPoseInternal(Handle, value);
        }
        private nint _isBindPose;

        public FbxPose(FbxManager manager, string name)
            : this(CreateFromManager(manager.Handle, name))
        {
        }

        public FbxPose(nint InHandle)
            : base(InHandle)
        {
            _isBindPose = GetPropertyPtr(0x78);
        }

        public FbxPose(FbxObject obj, string name)
            : this (CreateFromObject(obj.Handle, name))
        {
        }

        public int Add(FbxNode node, FbxMatrix matrix, bool localMatrix = false, bool multipleBindPose = false)
        {
            return AddInternal(Handle, node.Handle, matrix.Handle, localMatrix, multipleBindPose);
        }
    }

}
