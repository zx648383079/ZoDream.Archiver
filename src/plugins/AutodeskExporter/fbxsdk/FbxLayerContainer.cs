using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxLayerContainer : FbxNodeAttribute
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?GetLayer@FbxLayerContainer@fbxsdk@@QEAAPEAVFbxLayer@2@H@Z")]
        private static extern nint GetLayerInternal(nint inHandle, int pIndex);

        [DllImport(NativeMethods.DllName, EntryPoint = "?CreateLayer@FbxLayerContainer@fbxsdk@@QEAAHXZ")]
        private static extern int CreateLayerInternal(nint inHandle);
        [DllImport(NativeMethods.DllName, EntryPoint = "?GetLayerCount@FbxLayerContainer@fbxsdk@@QEBAHXZ")]
        private static extern long GetLayerCountInternal(nint inHandle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetLayerCount@FbxLayerContainer@fbxsdk@@QEBAHW4EType@FbxLayerElement@2@_N@Z")]
        private static extern long GetLayerCountInternal(nint inHandle, FbxLayerElement.EType pType, bool pUVCount = false);
        public int LayerCount => (int)GetLayerCountInternal(Handle);

        public FbxLayerContainer() { }
        public FbxLayerContainer(nint inHandle)
            : base(inHandle)
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

        public int GetLayerCount(FbxLayerElement.EType pType, bool pUVCount = false)
        {
            return (int)GetLayerCountInternal(Handle, pType, pUVCount);
        }

        public IEnumerable<FbxLayer> GetLayers()
        {
            var count = LayerCount;
            for (int i = 0; i < count; i++)
            {
                var res = GetLayer(i);
                if (res != null)
                {
                    yield return res;
                }
            }
        }
    }

}
