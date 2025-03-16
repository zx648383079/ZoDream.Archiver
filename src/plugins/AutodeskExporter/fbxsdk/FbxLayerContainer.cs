using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxLayerContainer : FbxNodeAttribute
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?GetLayer@FbxLayerContainer@fbxsdk@@QEBAPEBVFbxLayer@2@H@Z", CallingConvention = CallingConvention.ThisCall)]
        private static extern nint GetLayerInternal(nint InHandle, int pIndex);

        [DllImport(NativeMethods.DllName, EntryPoint = "?CreateLayer@FbxLayerContainer@fbxsdk@@QEAAHXZ", CallingConvention = CallingConvention.ThisCall)]
        private static extern int CreateLayerInternal(nint InHandle);

        public FbxLayerContainer() { }
        public FbxLayerContainer(nint InHandle)
            : base(InHandle)
        {
        }

        public FbxLayer? GetLayer(int pIndex)
        {
            nint Ptr = GetLayerInternal(Handle, pIndex);
            return Ptr == nint.Zero ? null : new FbxLayer(Ptr);
        }

        public int CreateLayer()
        {
            return CreateLayerInternal(Handle);
        }
    }

}
