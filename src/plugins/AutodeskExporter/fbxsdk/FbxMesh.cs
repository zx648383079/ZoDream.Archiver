using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxMesh : FbxGeometry
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxMesh@fbxsdk@@SAPEAV12@PEAVFbxManager@2@PEBD@Z")]
        private static extern IntPtr CreateFromManager(IntPtr pManager, [MarshalAs(UnmanagedType.LPStr)] string pName);

        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxMesh@fbxsdk@@SAPEAV12@PEAVFbxObject@2@PEBD@Z")]
        private static extern IntPtr CreateFromObject(IntPtr pObject, [MarshalAs(UnmanagedType.LPStr)] string pName);

        [DllImport(NativeMethods.DllName, EntryPoint = "?BeginPolygon@FbxMesh@fbxsdk@@QEAAXHHH_N@Z")]
        private static extern void BeginPolygonInternal(IntPtr InHandle, int pMaterial, int pTexture, int pGroup, bool bLegacy);

        [DllImport(NativeMethods.DllName, EntryPoint = "?AddPolygon@FbxMesh@fbxsdk@@QEAAXHH@Z")]
        private static extern void AddPolygonInternal(IntPtr InHandle, int pIndex, int pTextureUVIndex);

        [DllImport(NativeMethods.DllName, EntryPoint = "?EndPolygon@FbxMesh@fbxsdk@@QEAAXXZ")]
        private static extern void EndPolygonInternal(IntPtr InHandle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetPolygonCount@FbxMesh@fbxsdk@@QEBAHXZ")]
        private static extern int GetPolygonCountInternal(IntPtr handle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetPolygonSize@FbxMesh@fbxsdk@@QEBAHH@Z")]
        private static extern int GetPolygonSizeInternal(IntPtr handle, int pIndex);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetPolygonVertex@FbxMesh@fbxsdk@@QEBAHHH@Z")]
        private static extern int GetPolygonIndexInternal(IntPtr handle, int pPolygonIndex, int pPositionInPolygon);

        [DllImport(NativeMethods.DllName, EntryPoint = "?IsTriangleMesh@FbxMesh@fbxsdk@@QEBA_NXZ")]
        private static extern bool IsTriangleMeshInternal(IntPtr handle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?RemoveBadPolygons@FbxMesh@fbxsdk@@QEAAHXZ")]
        private static extern int RemoveBadPolygonsInternal(IntPtr handle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetMeshEdgeCount@FbxMesh@fbxsdk@@QEBAHXZ")]
        private static extern int GetMeshEdgeCountInternal(IntPtr handle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?BuildMeshEdgeArray@FbxMesh@fbxsdk@@QEAAXXZ")]
        private static extern void BuildMeshEdgeArrayInternal(IntPtr handle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?BeginGetMeshEdgeVertices@FbxMesh@fbxsdk@@QEAAXXZ")]
        private static extern void BeginGetMeshEdgeVerticesInternal(IntPtr handle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?EndGetMeshEdgeVertices@FbxMesh@fbxsdk@@QEAAXXZ")]
        private static extern void EndGetMeshEdgeVerticesInternal(IntPtr handle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?EndGetMeshEdgeVertices@FbxMesh@fbxsdk@@QEAAXXZ")]
        private static extern void GetMeshEdgeVerticesInternal(IntPtr handle, int pEdgeIndex, ref int pStartVertexIndex, ref int pEndVertexIndex);

        [DllImport(NativeMethods.DllName, EntryPoint = "?BeginGetMeshEdgeIndexForPolygon@FbxMesh@fbxsdk@@QEAAXXZ")]
        private static extern void BeginGetMeshEdgeIndexForPolygonInternal(IntPtr handle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?EndGetMeshEdgeIndexForPolygon@FbxMesh@fbxsdk@@QEAAXXZ")]
        private static extern void EndGetMeshEdgeIndexForPolygonInternal(IntPtr handle);


        public int PolygonCount => GetPolygonCountInternal(pHandle);
        public int MeshEdgeCount => GetMeshEdgeCountInternal(pHandle);

        public FbxMesh(FbxManager mgr, string name)
            : base(CreateFromManager(mgr.Handle, name))
        {
        }

        public FbxMesh(FbxObject obj, string name)
            : base(CreateFromObject(obj.Handle, name))
        {
        }

        public FbxMesh(FbxNodeAttribute attr)
            : base(attr.Handle)
        {
        }

        public FbxMesh(nint handle)
            : base(handle)
        {
        }

        public void BeginPolygon(int materialIndex = -1)
        {
            BeginPolygonInternal(pHandle, materialIndex, -1, -1, true);
        }

        public void AddPolygon(int index)
        {
            AddPolygonInternal(pHandle, index, -1);
        }

        public void EndPolygon()
        {
            EndPolygonInternal(pHandle);
        }

        public int GetPolygonSize(int index)
        {
            return GetPolygonSizeInternal(pHandle, index);
        }

        public int GetPolygonIndex(int index, int position)
        {
            return GetPolygonIndexInternal(pHandle, index, position);
        }

        public bool IsTriangleMesh()
        {
            return IsTriangleMeshInternal(pHandle);
        }

        public int RemoveBadPolygons()
        {
            return RemoveBadPolygonsInternal(pHandle);
        }

        public void BuildMeshEdgeArray()
        {
            BuildMeshEdgeArrayInternal(pHandle);
        }

        public void BeginGetMeshEdgeVertices()
        {
            BeginGetMeshEdgeVerticesInternal(pHandle);
        }

        public void EndGetMeshEdgeVertices()
        {
            EndGetMeshEdgeVerticesInternal(pHandle);
        }

        public void GetMeshEdgeVertices(int pEdgeIndex, ref int pStartVertexIndex, ref int pEndVertexIndex)
        {
            GetMeshEdgeVerticesInternal(pHandle, pEdgeIndex, ref pStartVertexIndex, ref pEndVertexIndex);
        }

        public void BeginGetMeshEdgeIndexForPolygon()
        {
            BeginGetMeshEdgeIndexForPolygonInternal(pHandle);
        }

        public void EndGetMeshEdgeIndexForPolygon()
        {
            EndGetMeshEdgeIndexForPolygonInternal(pHandle);
        }


    }

}
