using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxGeometryBase : FbxLayerContainer
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?InitControlPoints@FbxGeometryBase@fbxsdk@@UEAAXH@Z")]
        private static extern void InitControlPointsInternal(IntPtr InHandle, int pCount);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetControlPoints@FbxGeometryBase@fbxsdk@@UEBAPEAVFbxVector4@2@PEAVFbxStatus@2@@Z")]
        private static extern IntPtr GetControlPointsInternal(IntPtr InHandle, IntPtr pStatus);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetControlPointsCount@FbxGeometryBase@fbxsdk@@UEBAHXZ")]
        private static extern int GetControlsPointsCount(IntPtr handle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetElementNormalCount@FbxGeometryBase@fbxsdk@@QEBAHXZ")]
        private static extern int GetElementNormalCountInternal(IntPtr handle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetElementTangentCount@FbxGeometryBase@fbxsdk@@QEBAHXZ")]
        private static extern int GetElementTangentCountInternal(IntPtr handle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetElementBinormalCount@FbxGeometryBase@fbxsdk@@QEBAHXZ")]
        private static extern int GetElementBinormalCountInternal(IntPtr handle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetElementUVCount@FbxGeometryBase@fbxsdk@@QEBAHW4EType@FbxLayerElement@2@@Z")]
        private static extern int GetElementUVCountInternal(IntPtr handle, FbxLayerElement.EType type);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetElementVertexColorCount@FbxGeometryBase@fbxsdk@@QEBAHXZ")]
        private static extern int GetElementVertexColorCountInternal(IntPtr handle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetElementNormal@FbxGeometryBase@fbxsdk@@QEAAPEAVFbxLayerElementNormal@2@H@Z")]
        private static extern IntPtr GetElementNormalInternal(IntPtr handle, int pIndex);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetElementTangent@FbxGeometryBase@fbxsdk@@QEAAPEAVFbxLayerElementTangent@2@H@Z")]
        private static extern IntPtr GetElementTangentInternal(IntPtr handle, int pIndex);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetElementBinormal@FbxGeometryBase@fbxsdk@@QEAAPEAVFbxLayerElementBinormal@2@H@Z")]
        private static extern IntPtr GetElementBinormalInternal(IntPtr handle, int pIndex);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetElementUV@FbxGeometryBase@fbxsdk@@QEAAPEAVFbxLayerElementUV@2@HW4EType@FbxLayerElement@2@@Z")]
        private static extern IntPtr GetElementUVInternal(IntPtr handle, int pIndex, FbxLayerElement.EType pType);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetElementVertexColor@FbxGeometryBase@fbxsdk@@QEAAPEAVFbxLayerElementVertexColor@2@H@Z")]
        private static extern IntPtr GetElementVertexColorInternal(IntPtr handle, int pIndex);

        [DllImport(NativeMethods.DllName, EntryPoint = "?SetControlPointAt@FbxGeometryBase@fbxsdk@@UEAAXAEBVFbxVector4@2@0H_N@Z")]
        private static extern IntPtr SetControlPointAtInternal(IntPtr handle, nint pCtrlPoint, int pIndex);
        [DllImport(NativeMethods.DllName, EntryPoint = "?InitNormals@FbxGeometryBase@fbxsdk@@QEAAXPEAV12@@Z")]
        private static extern IntPtr InitNormalsInternal(IntPtr handle, nint pSrc);
        [DllImport(NativeMethods.DllName, EntryPoint = "?SetControlPointNormalAt@FbxGeometryBase@fbxsdk@@UEAAXAEBVFbxVector4@2@H_N@Z")]
        private static extern IntPtr SetControlPointNormalAtInternal(IntPtr handle, nint pNormal, int pIndex);

        [DllImport(NativeMethods.DllName, EntryPoint = "?CreateElementMaterial@FbxGeometryBase@fbxsdk@@QEAAPEAVFbxLayerElementMaterial@2@XZ")]
        private static extern nint CreateElementMaterialInternal(nint pHandle);
        [DllImport(NativeMethods.DllName, EntryPoint = "?CreateElementVertexColor@FbxGeometryBase@fbxsdk@@QEAAPEAVFbxLayerElementVertexColor@2@XZ")]
        private static extern nint CreateElementVertexColorInternal(nint pHandle);
        [DllImport(NativeMethods.DllName, EntryPoint = "?CreateElementTangent@FbxGeometryBase@fbxsdk@@QEAAPEAVFbxLayerElementTangent@2@XZ")]
        private static extern nint CreateElementTangentInternal(nint pHandle);
        [DllImport(NativeMethods.DllName, EntryPoint = "?CreateElementUV@FbxGeometryBase@fbxsdk@@QEAAPEAVFbxLayerElementUV@2@PEBDW4EType@FbxLayerElement@2@@Z")]
        private static extern nint CreateElementUVInternal(nint pHandle, [MarshalAs(UnmanagedType.LPStr)] string pUVSetName, FbxLayerElement.EType pTypeIdentifier);


        public int ControlPointsCount => GetControlsPointsCount(pHandle);
        public int ElementNormalCount => GetElementNormalCountInternal(pHandle);
        public int ElementTangentCount => GetElementTangentCountInternal(pHandle);
        public int ElementBinormalCount => GetElementBinormalCountInternal(pHandle);
        public int ElementUVCount => GetElementUVCountInternal(pHandle, FbxLayerElement.EType.eUnknown);
        public int ElementVertexColorCount => GetElementVertexColorCountInternal(pHandle);

        public FbxGeometryBase() { }
        public FbxGeometryBase(IntPtr InHandle)
            : base(InHandle)
        {
        }

        public void InitControlPoints(int pCount)
        {
            InitControlPointsInternal(pHandle, pCount);
        }

        internal void SetControlPointAt(Vector4 pCtrlPoint, int pIndex)
        {
            IntPtr ptr = FbxDouble4.Construct(pCtrlPoint);
            SetControlPointAtInternal(pHandle, ptr, pIndex);
            FbxUtils.FbxFree(ptr);

        }

        internal void InitNormals(FbxGeometryBase pSrc)
        {
            InitNormalsInternal(pHandle, pSrc.pHandle);
        }

        internal void SetControlPointNormalAt(Vector4 pNormal, int pIndex)
        {
            IntPtr ptr = FbxDouble4.Construct(pNormal);
            SetControlPointNormalAtInternal(pHandle, ptr, pIndex);
            FbxUtils.FbxFree(ptr);
        }

        public FbxArray<Vector4> GetControlPoints()
        {
            IntPtr Ptr = GetControlPointsInternal(pHandle, IntPtr.Zero);
            return new FbxArray<Vector4>(Ptr);
        }

        public FbxLayerElementTangent? GetElementTangent(int index)
        {
            IntPtr ptr = GetElementTangentInternal(pHandle, index);
            return ptr == IntPtr.Zero ? null : new FbxLayerElementTangent(ptr);
        }

        public FbxLayerElementBinormal? GetElementBinormal(int index)
        {
            IntPtr ptr = GetElementBinormalInternal(pHandle, index);
            return ptr == IntPtr.Zero ? null : new FbxLayerElementBinormal(ptr);
        }

        public FbxLayerElementNormal? GetElementNormal(int index)
        {
            IntPtr ptr = GetElementNormalInternal(pHandle, index);
            return ptr == IntPtr.Zero ? null : new FbxLayerElementNormal(ptr);
        }

        public FbxLayerElementUV? GetElementUV(int index, FbxLayerElement.EType type)
        {
            IntPtr ptr = GetElementUVInternal(pHandle, index, type);
            return ptr == IntPtr.Zero ? null : new FbxLayerElementUV(ptr);
        }

        public FbxLayerElementUV? GetElementUV(string name)
        {
            for (int i = 0; i < ElementUVCount; i++)
            {
                var layer = GetElementUV(i, FbxLayerElement.EType.eUnknown);
                if (layer?.Name == name)
                {
                    return layer;
                }
            }
            return null;
        }

        public FbxLayerElementVertexColor? GetElementVertexColor(int index)
        {
            IntPtr ptr = GetElementVertexColorInternal(pHandle, index);
            return ptr == IntPtr.Zero ? null : new FbxLayerElementVertexColor(ptr);
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
            var ptr = CreateElementMaterialInternal(pHandle);
            return new FbxLayerElementMaterial(ptr);
        }



        internal FbxLayerElementVertexColor CreateElementVertexColor()
        {
            var ptr = CreateElementVertexColorInternal(pHandle);
            return new FbxLayerElementVertexColor(ptr);
        }



        internal FbxLayerElementTangent CreateElementTangent()
        {
            var ptr = CreateElementTangentInternal(pHandle);
            return new FbxLayerElementTangent(ptr);
        }

        internal FbxLayerElementUV CreateElementUV(string pUVSetName, FbxLayerElement.EType pTypeIdentifier)
        {
            var ptr = CreateElementUVInternal(pHandle, pUVSetName, pTypeIdentifier);
            return new FbxLayerElementUV(ptr);
        }
    }

}
