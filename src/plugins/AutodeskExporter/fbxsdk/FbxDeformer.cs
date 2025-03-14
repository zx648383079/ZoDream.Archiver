﻿using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxDeformer : FbxObject
    {
        public enum EDeformerType
        {
            eUnknown,
            eSkin,
            eBlendShape,
            eVertexCache
        };

        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxDeformer@fbxsdk@@SAPEAV12@PEAVFbxManager@2@PEBD@Z")]
        private static extern IntPtr CreateFromManager(IntPtr pManager, [MarshalAs(UnmanagedType.LPStr)] string pName);

        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxDeformer@fbxsdk@@SAPEAV12@PEAVFbxObject@2@PEBD@Z")]
        private static extern IntPtr CreateFromObject(IntPtr pObject, [MarshalAs(UnmanagedType.LPStr)] string pName);

        private delegate EDeformerType GetDeformerTypeDelegate(IntPtr handle);
        private GetDeformerTypeDelegate GetDeformerTypeInternal;

        public EDeformerType DeformerType => GetDeformerTypeInternal(pHandle);

        public FbxDeformer(IntPtr InHandle)
            : base(InHandle)
        {
            GetDeformerTypeInternal = Marshal.GetDelegateForFunctionPointer<GetDeformerTypeDelegate>(Marshal.ReadIntPtr(vTable + 0xB8));
        }

        public FbxDeformer(FbxManager manager, string name)
            : this(CreateFromManager(manager.Handle, name))
        {
        }

        public FbxDeformer(FbxObject obj, string name)
            : this(CreateFromObject(obj.Handle, name))
        {
        }
    }

}
