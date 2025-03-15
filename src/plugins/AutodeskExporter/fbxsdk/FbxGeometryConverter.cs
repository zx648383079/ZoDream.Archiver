using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxGeometryConverter : FbxNative
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "??0FbxGeometryConverter@fbxsdk@@QEAA@PEAVFbxManager@1@@Z")]
        private static extern void CreateFromManager(IntPtr handle, IntPtr manager);

        [DllImport(NativeMethods.DllName, EntryPoint = "??1FbxGeometryConverter@fbxsdk@@QEAA@XZ")]
        private static extern void DisposeInternal(IntPtr handle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?ComputeEdgeSmoothingFromNormals@FbxGeometryConverter@fbxsdk@@QEBA_NPEAVFbxMesh@2@@Z")]
        private static extern bool ComputeEdgeSmoothingFromNormalsInternal(IntPtr handle, IntPtr mesh);

        public FbxGeometryConverter(FbxManager mgr)
            : base(FbxUtils.FbxMalloc(16))
        {
            CreateFromManager(Handle, mgr.Handle);
        }

        public bool ComputeEdgeSmoothingFromNormals(FbxMesh pMesh)
        {
            return ComputeEdgeSmoothingFromNormalsInternal(Handle, pMesh.Handle);
        }

        protected override void Dispose(bool disposing)
        {
            DisposeInternal(Handle);
            base.Dispose(disposing);
        }
    }

}
