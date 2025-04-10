﻿using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxLayerElementUserData : FbxLayerElement
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxLayerElementUserData@fbxsdk@@SAPEAV12@PEAVFbxLayerContainer@2@AEBV12@@Z")]
        private static extern nint CreateFromObject(nint pOwner, [MarshalAs(UnmanagedType.LPStr)] string pName);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetDirectArray@?$FbxLayerElementTemplate@VFbxColor@fbxsdk@@@fbxsdk@@QEAAAEAV?$FbxLayerElementArrayTemplate@VFbxColor@fbxsdk@@@2@XZ", CallingConvention = CallingConvention.ThisCall)]
        private static extern nint GetDirectArrayInternal(nint InHandle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetIndexArray@?$FbxLayerElementTemplate@VFbxColor@fbxsdk@@@fbxsdk@@QEAAAEAV?$FbxLayerElementArrayTemplate@H@2@XZ", CallingConvention = CallingConvention.ThisCall)]
        private static extern nint GetIndexArrayInternal(nint InHandle);

        public FbxLayerElementUserData(FbxLayerContainer pOwner, string pName)
            : base(CreateFromObject(pOwner.Handle, pName))
        {
        }

        public FbxLayerElementArray? DirectArray {
            get {
                var ptr = GetDirectArrayInternal(Handle);
                return ptr == nint.Zero ? null : new FbxLayerElementArray(ptr);
            }
        }

        public FbxLayerElementArray? IndexArray {
            get {
                var ptr = GetIndexArrayInternal(Handle);
                return ptr == nint.Zero ? null : new FbxLayerElementArray(ptr);
            }
        }
    }

}
