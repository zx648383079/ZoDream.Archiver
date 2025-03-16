using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxLayer : FbxNative
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?SetNormals@FbxLayer@fbxsdk@@QEAAXPEAVFbxLayerElementNormal@2@@Z", CallingConvention = CallingConvention.ThisCall)]
        private static extern void SetNormalsInternal(nint InHandle, nint pNormals);

        [DllImport(NativeMethods.DllName, EntryPoint = "?SetTangents@FbxLayer@fbxsdk@@QEAAXPEAVFbxLayerElementTangent@2@@Z", CallingConvention = CallingConvention.ThisCall)]
        private static extern void SetTangentsInternal(nint InHandle, nint pTangents);

        [DllImport(NativeMethods.DllName, EntryPoint = "?SetBinormals@FbxLayer@fbxsdk@@QEAAXPEAVFbxLayerElementBinormal@2@@Z", CallingConvention = CallingConvention.ThisCall)]
        private static extern void SetBinormalsInternal(nint InHandle, nint pBinormals);

        [DllImport(NativeMethods.DllName, EntryPoint = "?SetUVs@FbxLayer@fbxsdk@@QEAAXPEAVFbxLayerElementUV@2@W4EType@FbxLayerElement@2@@Z", CallingConvention = CallingConvention.ThisCall)]
        private static extern void SetUVsInternal(nint InHandle, nint pUVs, FbxLayerElement.EType pTypeIdentifier);

        [DllImport(NativeMethods.DllName, EntryPoint = "?SetVertexColors@FbxLayer@fbxsdk@@QEAAXPEAVFbxLayerElementVertexColor@2@@Z", CallingConvention = CallingConvention.ThisCall)]
        private static extern void SetVertexColorsInternal(nint InHandle, nint pVertexColors);

        [DllImport(NativeMethods.DllName, EntryPoint = "?SetUserData@FbxLayer@fbxsdk@@QEAAXPEAVFbxLayerElementUserData@2@@Z", CallingConvention = CallingConvention.ThisCall)]
        private static extern void SetUserDataInternal(nint InHandle, nint pUserData);
        [DllImport(NativeMethods.DllName, EntryPoint = "?GetUVs@FbxLayer@fbxsdk@@QEAAPEAVFbxLayerElementUV@2@W4EType@FbxLayerElement@2@@Z")]
        private static extern nint GetUvsInternal(nint handle, FbxLayerElement.EType eType);

        public FbxLayer(nint InHandle)
            : base(InHandle)
        {
        }

        public void SetNormals(FbxLayerElementNormal pNormals)
        {
            SetNormalsInternal(Handle, pNormals.Handle);
        }

        public void SetTangents(FbxLayerElementTangent pTangents)
        {
            SetTangentsInternal(Handle, pTangents.Handle);
        }

        public void SetBinormals(FbxLayerElementBinormal pBinormals)
        {
            SetBinormalsInternal(Handle, pBinormals.Handle);
        }

        public void SetUVs(FbxLayerElementUV pUVs)
        {
            SetUVsInternal(Handle, pUVs.Handle, FbxLayerElement.EType.eTextureDiffuse);
        }

        public void SetVertexColors(FbxLayerElementVertexColor pVertexColors)
        {
            SetVertexColorsInternal(Handle, pVertexColors.Handle);
        }

        public void SetUserData(FbxLayerElementUserData pUserData)
        {
            SetUserDataInternal(Handle, pUserData.Handle);
        }

        public T? GetUvs<T>(FbxLayerElement.EType eType)
            where T : FbxLayerElement
        {
            var ptr = GetUvsInternal(Handle, eType);
            if (ptr == nint.Zero)
            {
                return null;
            }
            return (T)Activator.CreateInstance(typeof(T), ptr);
        }

    }

}
