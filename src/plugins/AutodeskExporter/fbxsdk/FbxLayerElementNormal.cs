using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxLayerElementNormal : FbxLayerElement
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxLayerElementNormal@fbxsdk@@SAPEAV12@PEAVFbxLayerContainer@2@PEBD@Z")]
        private static extern nint CreateFromObject(nint pOwner, [MarshalAs(UnmanagedType.LPStr)] string pName);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetDirectArray@?$FbxLayerElementTemplate@VFbxVector4@fbxsdk@@@fbxsdk@@QEAAAEAV?$FbxLayerElementArrayTemplate@VFbxVector4@fbxsdk@@@2@XZ", CallingConvention = CallingConvention.ThisCall)]
        private static extern nint GetDirectArrayInternal(nint InHandle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetIndexArray@?$FbxLayerElementTemplate@VFbxVector4@fbxsdk@@@fbxsdk@@QEAAAEAV?$FbxLayerElementArrayTemplate@H@2@XZ", CallingConvention = CallingConvention.ThisCall)]
        private static extern nint GetIndexArrayInternal(nint InHandle);

        public FbxLayerElementNormal(nint handle)
            : base(handle)
        {
        }

        public FbxLayerElementNormal(FbxLayerContainer pOwner, string pName)
            : this(CreateFromObject(pOwner.Handle, pName))
        {
        }

        public FbxLayerElementArray? DirectArray {
            get {
                nint Ptr = GetDirectArrayInternal(Handle);
                return Ptr == nint.Zero ? null : new FbxLayerElementArray(Ptr);
            }
        }

        public FbxLayerElementArray? IndexArray {
            get {
                nint Ptr = GetIndexArrayInternal(Handle);
                return Ptr == nint.Zero ? null : new FbxLayerElementArray(Ptr);
            }
        }
    }

}
