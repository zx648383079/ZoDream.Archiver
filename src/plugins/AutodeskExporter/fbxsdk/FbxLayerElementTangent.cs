using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxLayerElementTangent : FbxLayerElement
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxLayerElementTangent@fbxsdk@@SAPEAV12@PEAVFbxLayerContainer@2@PEBD@Z")]
        private static extern IntPtr CreateFromObject(IntPtr pOwner, [MarshalAs(UnmanagedType.LPStr)] string pName);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetDirectArray@?$FbxLayerElementTemplate@VFbxVector4@fbxsdk@@@fbxsdk@@QEAAAEAV?$FbxLayerElementArrayTemplate@VFbxVector4@fbxsdk@@@2@XZ", CallingConvention = CallingConvention.ThisCall)]
        private static extern IntPtr GetDirectArrayInternal(IntPtr InHandle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetIndexArray@?$FbxLayerElementTemplate@VFbxVector4@fbxsdk@@@fbxsdk@@QEAAAEAV?$FbxLayerElementArrayTemplate@H@2@XZ", CallingConvention = CallingConvention.ThisCall)]
        private static extern IntPtr GetIndexArrayInternal(IntPtr InHandle);

        public FbxLayerElementTangent(IntPtr handle)
            : base(handle)
        {
        }

        public FbxLayerElementTangent(FbxLayerContainer pOwner, string pName)
            : this(CreateFromObject(pOwner.Handle, pName))
        {
        }

        public FbxLayerElementArray? DirectArray {
            get {
                IntPtr Ptr = GetDirectArrayInternal(Handle);
                return Ptr == IntPtr.Zero ? null : new FbxLayerElementArray(Ptr);
            }
        }

        public FbxLayerElementArray? IndexArray {
            get {
                IntPtr Ptr = GetIndexArrayInternal(Handle);
                return Ptr == IntPtr.Zero ? null : new FbxLayerElementArray(Ptr);
            }
        }
    }

}
