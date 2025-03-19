using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxGeometryBase : FbxLayerContainer
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?InitControlPoints@FbxGeometryBase@fbxsdk@@UEAAXH@Z")]
        private static extern void InitControlPointsInternal(nint InHandle, int pCount);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetControlPoints@FbxGeometryBase@fbxsdk@@UEBAPEAVFbxVector4@2@PEAVFbxStatus@2@@Z")]
        private static extern nint GetControlPointsInternal(nint InHandle, nint pStatus);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetControlPointsCount@FbxGeometryBase@fbxsdk@@UEBAHXZ")]
        private static extern int GetControlsPointsCount(nint handle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetElementNormalCount@FbxGeometryBase@fbxsdk@@QEBAHXZ")]
        private static extern int GetElementNormalCountInternal(nint handle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetElementTangentCount@FbxGeometryBase@fbxsdk@@QEBAHXZ")]
        private static extern int GetElementTangentCountInternal(nint handle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetElementBinormalCount@FbxGeometryBase@fbxsdk@@QEBAHXZ")]
        private static extern int GetElementBinormalCountInternal(nint handle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetElementUVCount@FbxGeometryBase@fbxsdk@@QEBAHW4EType@FbxLayerElement@2@@Z")]
        private static extern int GetElementUVCountInternal(nint handle, FbxLayerElement.EType type);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetElementVertexColorCount@FbxGeometryBase@fbxsdk@@QEBAHXZ")]
        private static extern int GetElementVertexColorCountInternal(nint handle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetElementNormal@FbxGeometryBase@fbxsdk@@QEAAPEAVFbxLayerElementNormal@2@H@Z")]
        private static extern nint GetElementNormalInternal(nint handle, int pIndex);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetElementTangent@FbxGeometryBase@fbxsdk@@QEAAPEAVFbxLayerElementTangent@2@H@Z")]
        private static extern nint GetElementTangentInternal(nint handle, int pIndex);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetElementBinormal@FbxGeometryBase@fbxsdk@@QEAAPEAVFbxLayerElementBinormal@2@H@Z")]
        private static extern nint GetElementBinormalInternal(nint handle, int pIndex);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetElementUV@FbxGeometryBase@fbxsdk@@QEAAPEAVFbxLayerElementUV@2@HW4EType@FbxLayerElement@2@@Z")]
        private static extern nint GetElementUVInternal(nint handle, int pIndex, FbxLayerElement.EType pType);
        [DllImport(NativeMethods.DllName, CharSet = CharSet.Auto, EntryPoint = "?GetElementUV@FbxGeometryBase@fbxsdk@@QEAAPEAVFbxLayerElementUV@2@PEBD@Z")]
        private static extern nint GetElementUVInternal(nint handle, [MarshalAs(UnmanagedType.LPStr)] string name);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetElementVertexColor@FbxGeometryBase@fbxsdk@@QEAAPEAVFbxLayerElementVertexColor@2@H@Z")]
        private static extern nint GetElementVertexColorInternal(nint handle, int pIndex);

        [DllImport(NativeMethods.DllName, EntryPoint = "?SetControlPointAt@FbxGeometryBase@fbxsdk@@UEAAXAEBVFbxVector4@2@H@Z")]
        private static extern nint SetControlPointAtInternal(nint handle, nint pCtrlPoint, int pIndex);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetControlPointAt@FbxGeometryBase@fbxsdk@@UEBA?AVFbxVector4@2@H@Z")]
        private static extern nint GetControlPointAtInternal(nint handle, nint pCtrlPoint, int pIndex);


        [DllImport(NativeMethods.DllName, EntryPoint = "?InitNormals@FbxGeometryBase@fbxsdk@@QEAAXPEAV12@@Z")]
        private static extern nint InitNormalsInternal(nint handle, nint pSrc);
        [DllImport(NativeMethods.DllName, EntryPoint = "?SetControlPointNormalAt@FbxGeometryBase@fbxsdk@@UEAAXAEBVFbxVector4@2@H_N@Z")]
        private static extern nint SetControlPointNormalAtInternal(nint handle, nint pNormal, int pIndex);

        [DllImport(NativeMethods.DllName, EntryPoint = "?CreateElementMaterial@FbxGeometryBase@fbxsdk@@QEAAPEAVFbxLayerElementMaterial@2@XZ")]
        private static extern nint CreateElementMaterialInternal(nint pHandle);
        [DllImport(NativeMethods.DllName, EntryPoint = "?CreateElementVertexColor@FbxGeometryBase@fbxsdk@@QEAAPEAVFbxLayerElementVertexColor@2@XZ")]
        private static extern nint CreateElementVertexColorInternal(nint pHandle);
        [DllImport(NativeMethods.DllName, EntryPoint = "?CreateElementTangent@FbxGeometryBase@fbxsdk@@QEAAPEAVFbxLayerElementTangent@2@XZ")]
        private static extern nint CreateElementTangentInternal(nint pHandle);
        [DllImport(NativeMethods.DllName, CharSet = CharSet.Auto, EntryPoint = "?CreateElementUV@FbxGeometryBase@fbxsdk@@QEAAPEAVFbxLayerElementUV@2@PEBDW4EType@FbxLayerElement@2@@Z")]
        private static extern nint CreateElementUVInternal(nint pHandle, [MarshalAs(UnmanagedType.LPStr)] string pUVSetName, FbxLayerElement.EType pTypeIdentifier);

        [DllImport(NativeMethods.DllName, EntryPoint = "?CreateElementNormal@FbxGeometryBase@fbxsdk@@QEAAPEAVFbxLayerElementNormal@2@XZ")]
        private static extern nint CreateElementNormalInternal(nint pHandle);
        public int ControlPointsCount => GetControlsPointsCount(Handle);
        public int ElementNormalCount => GetElementNormalCountInternal(Handle);
        public int ElementTangentCount => GetElementTangentCountInternal(Handle);
        public int ElementBinormalCount => GetElementBinormalCountInternal(Handle);
        public int ElementUVCount => GetElementUVCountInternal(Handle, FbxLayerElement.EType.eUnknown);
        public int ElementVertexColorCount => GetElementVertexColorCountInternal(Handle);

        public FbxGeometryBase() { }
        public FbxGeometryBase(nint InHandle)
            : base(InHandle)
        {
        }

        public void InitControlPoints(int pCount)
        {
            InitControlPointsInternal(Handle, pCount);
        }

        internal void SetControlPointAt(int pIndex, Vector4 pCtrlPoint)
        {
            using var ptr = new FbxVector4(pCtrlPoint);
            SetControlPointAt(pIndex, ptr);
        }

        internal void SetControlPointAt(int pIndex, FbxVector4 pCtrlPoint)
        {
            SetControlPointAtInternal(Handle, pCtrlPoint.Handle, pIndex);
        }

        internal void InitNormals(FbxGeometryBase pSrc)
        {
            InitNormalsInternal(Handle, pSrc.Handle);
        }

        internal void SetControlPointNormalAt(int pIndex, Vector4 pNormal)
        {
            using var ptr = new FbxVector4(pNormal);
            SetControlPointNormalAt(pIndex, ptr);
        }

        internal void SetControlPointNormalAt(int pIndex, FbxVector4 pNormal)
        {
            SetControlPointNormalAtInternal(Handle, pNormal.Handle, pIndex);
        }

        public FbxVector4 GetControlPointAt(int index)
        {
            var vec = new FbxVector4();
            GetControlPointAtInternal(Handle, vec.Handle, index);
            return vec;
        }


        public FbxLayerElementTangent? GetElementTangent(int index)
        {
            nint ptr = GetElementTangentInternal(Handle, index);
            return ptr == nint.Zero ? null : new FbxLayerElementTangent(ptr);
        }

        public FbxLayerElementBinormal? GetElementBinormal(int index)
        {
            nint ptr = GetElementBinormalInternal(Handle, index);
            return ptr == nint.Zero ? null : new FbxLayerElementBinormal(ptr);
        }

        public FbxLayerElementNormal? GetElementNormal(int index)
        {
            nint ptr = GetElementNormalInternal(Handle, index);
            return ptr == nint.Zero ? null : new FbxLayerElementNormal(ptr);
        }

        public FbxLayerElementUV? GetElementUV(int index, FbxLayerElement.EType type)
        {
            var ptr = GetElementUVInternal(Handle, index, type);
            return ptr == nint.Zero ? null : new FbxLayerElementUV(ptr);
        }

        public FbxLayerElementUV? GetElementUV(string name)
        {
            //foreach (var layer in GetLayers())
            //{
            //    for (var i = FbxLayerElement.EType.eTextureDiffuse;
            //        i < FbxLayerElement.EType.eTypeCount; i++)
            //    {
            //        var uv = layer.GetUvs<FbxLayerElementUV>(i);
            //        if (uv?.Name == name)
            //        {
            //            return uv;
            //        }
            //    }
            //}
            //return null;
            var ptr = GetElementUVInternal(Handle, name);
            return ptr == nint.Zero ? null : new FbxLayerElementUV(ptr);
        }

        public FbxLayerElementVertexColor? GetElementVertexColor(int index)
        {
            nint ptr = GetElementVertexColorInternal(Handle, index);
            return ptr == nint.Zero ? null : new FbxLayerElementVertexColor(ptr);
        }

        public FbxLayerElementVertexColor? GetElementVertexColor(string name)
        {
            for (int i = 0; i < ElementVertexColorCount; i++)
            {
                var layer = GetElementVertexColor(i);
                if (layer?.Name == name)
                {
                    return layer;
                }
            }
            return null;
        }

        internal FbxLayerElementMaterial CreateElementMaterial()
        {
            var ptr = CreateElementMaterialInternal(Handle);
            Debug.Assert(ptr != nint.Zero);
            return new FbxLayerElementMaterial(ptr);
        }



        internal FbxLayerElementVertexColor CreateElementVertexColor()
        {
            var ptr = CreateElementVertexColorInternal(Handle);
            Debug.Assert(ptr != nint.Zero);
            return new FbxLayerElementVertexColor(ptr);
        }

        internal FbxLayerElementNormal CreateElementNormal()
        {
            var ptr = CreateElementNormalInternal(Handle);
            Debug.Assert(ptr != nint.Zero);
            return new FbxLayerElementNormal(ptr);
        }

        internal FbxLayerElementTangent CreateElementTangent()
        {
            var ptr = CreateElementTangentInternal(Handle);
            Debug.Assert(ptr != nint.Zero);
            return new FbxLayerElementTangent(ptr);
        }

        internal FbxLayerElementUV CreateElementUV(string pUVSetName, FbxLayerElement.EType pTypeIdentifier)
        {
            //using var namePtr = new FbxString(pUVSetName);
            var ptr = CreateElementUVInternal(Handle, pUVSetName, pTypeIdentifier);
            Debug.Assert(ptr != nint.Zero);
            return new FbxLayerElementUV(ptr);
        }
    }

}
