using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxLayer : FbxNative
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?SetNormals@FbxLayer@fbxsdk@@QEAAXPEAVFbxLayerElementNormal@2@@Z", CallingConvention = CallingConvention.ThisCall)]
        private static extern void SetNormalsInternal(IntPtr InHandle, IntPtr pNormals);

        [DllImport(NativeMethods.DllName, EntryPoint = "?SetTangents@FbxLayer@fbxsdk@@QEAAXPEAVFbxLayerElementTangent@2@@Z", CallingConvention = CallingConvention.ThisCall)]
        private static extern void SetTangentsInternal(IntPtr InHandle, IntPtr pTangents);

        [DllImport(NativeMethods.DllName, EntryPoint = "?SetBinormals@FbxLayer@fbxsdk@@QEAAXPEAVFbxLayerElementBinormal@2@@Z", CallingConvention = CallingConvention.ThisCall)]
        private static extern void SetBinormalsInternal(IntPtr InHandle, IntPtr pBinormals);

        [DllImport(NativeMethods.DllName, EntryPoint = "?SetUVs@FbxLayer@fbxsdk@@QEAAXPEAVFbxLayerElementUV@2@W4EType@FbxLayerElement@2@@Z", CallingConvention = CallingConvention.ThisCall)]
        private static extern void SetUVsInternal(IntPtr InHandle, IntPtr pUVs, FbxLayerElement.EType pTypeIdentifier);

        [DllImport(NativeMethods.DllName, EntryPoint = "?SetVertexColors@FbxLayer@fbxsdk@@QEAAXPEAVFbxLayerElementVertexColor@2@@Z", CallingConvention = CallingConvention.ThisCall)]
        private static extern void SetVertexColorsInternal(IntPtr InHandle, IntPtr pVertexColors);

        [DllImport(NativeMethods.DllName, EntryPoint = "?SetUserData@FbxLayer@fbxsdk@@QEAAXPEAVFbxLayerElementUserData@2@@Z", CallingConvention = CallingConvention.ThisCall)]
        private static extern void SetUserDataInternal(IntPtr InHandle, IntPtr pUserData);

        public FbxLayer(IntPtr InHandle)
            : base(InHandle)
        {
        }

        public void SetNormals(FbxLayerElementNormal pNormals)
        {
            SetNormalsInternal(pHandle, pNormals.Handle);
        }

        public void SetTangents(FbxLayerElementTangent pTangents)
        {
            SetTangentsInternal(pHandle, pTangents.Handle);
        }

        public void SetBinormals(FbxLayerElementBinormal pBinormals)
        {
            SetBinormalsInternal(pHandle, pBinormals.Handle);
        }

        public void SetUVs(FbxLayerElementUV pUVs)
        {
            SetUVsInternal(pHandle, pUVs.Handle, FbxLayerElement.EType.eTextureDiffuse);
        }

        public void SetVertexColors(FbxLayerElementVertexColor pVertexColors)
        {
            SetVertexColorsInternal(pHandle, pVertexColors.Handle);
        }

        public void SetUserData(FbxLayerElementUserData pUserData)
        {
            SetUserDataInternal(pHandle, pUserData.Handle);
        }
    }

}
