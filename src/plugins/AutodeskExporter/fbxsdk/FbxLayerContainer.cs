using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxLayerContainer : FbxNodeAttribute
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?GetLayer@FbxLayerContainer@fbxsdk@@QEBAPEBVFbxLayer@2@H@Z", CallingConvention = CallingConvention.ThisCall)]
        private static extern IntPtr GetLayerInternal(IntPtr InHandle, int pIndex);

        [DllImport(NativeMethods.DllName, EntryPoint = "?CreateLayer@FbxLayerContainer@fbxsdk@@QEAAHXZ", CallingConvention = CallingConvention.ThisCall)]
        private static extern int CreateLayerInternal(IntPtr InHandle);

        public FbxLayerContainer() { }
        public FbxLayerContainer(IntPtr InHandle)
            : base(InHandle)
        {
        }

        public FbxLayer? GetLayer(int pIndex)
        {
            IntPtr Ptr = GetLayerInternal(pHandle, pIndex);
            return Ptr == IntPtr.Zero ? null : new FbxLayer(Ptr);
        }

        public int CreateLayer()
        {
            return CreateLayerInternal(pHandle);
        }
    }

}
