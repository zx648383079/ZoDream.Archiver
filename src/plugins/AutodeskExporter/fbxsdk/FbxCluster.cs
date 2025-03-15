using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxCluster : FbxSubDeformer
    {
        public enum ELinkMode
        {
            eNormalize,
            eAdditive,
            eTotalOne
        };

        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxCluster@fbxsdk@@SAPEAV12@PEAVFbxManager@2@PEBD@Z")]
        private static extern IntPtr CreateFromManager(IntPtr pManager, [MarshalAs(UnmanagedType.LPStr)] string pName);

        [DllImport(NativeMethods.DllName, EntryPoint = "?Create@FbxCluster@fbxsdk@@SAPEAV12@PEAVFbxObject@2@PEBD@Z")]
        private static extern IntPtr CreateFromObject(IntPtr pObject, [MarshalAs(UnmanagedType.LPStr)] string pName);

        [DllImport(NativeMethods.DllName, EntryPoint = "?SetLink@FbxCluster@fbxsdk@@QEAAXPEBVFbxNode@2@@Z")]
        private static extern void SetLinkInternal(IntPtr pHandle, IntPtr pNode);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetLinkMode@FbxCluster@fbxsdk@@QEBA?AW4ELinkMode@12@XZ")]
        private static extern ELinkMode GetLinkModeInternal(IntPtr handle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?SetLinkMode@FbxCluster@fbxsdk@@QEAAXW4ELinkMode@12@@Z")]
        private static extern void SetLinkModeInternal(IntPtr pHandle, ELinkMode pMode);

        [DllImport(NativeMethods.DllName, EntryPoint = "?AddControlPointIndex@FbxCluster@fbxsdk@@QEAAXHN@Z")]
        private static extern void AddControlPointIndexInternal(IntPtr pHandle, int pIndex, double pWeight);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetControlPointIndicesCount@FbxCluster@fbxsdk@@QEBAHXZ")]
        private static extern int GetControlPointIndicesCountInternal(IntPtr handle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetControlPointIndices@FbxCluster@fbxsdk@@QEBAPEAHXZ")]
        private static extern IntPtr GetControlPointIndicesInternal(IntPtr handle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetControlPointWeights@FbxCluster@fbxsdk@@QEBAPEANXZ")]
        private static extern IntPtr GetControlPointWeightsInternal(IntPtr handle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?SetTransformLinkMatrix@FbxCluster@fbxsdk@@QEAAXAEBVFbxAMatrix@2@@Z")]
        private static extern void SetTransformLinkMatrixInternal(IntPtr pHandle, IntPtr pMatrix);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetLink@FbxCluster@fbxsdk@@QEAAPEAVFbxNode@2@XZ")]
        private static extern IntPtr GetLinkInternal(IntPtr pHandle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?SetTransformMatrix@FbxCluster@fbxsdk@@QEAAXAEBVFbxAMatrix@2@@Z")]
        private static extern void SetTransformMatrixInternal(nint handle);

        public int ControlPointIndicesCount => GetControlPointIndicesCountInternal(Handle);
        public ELinkMode LinkMode { get => GetLinkModeInternal(Handle); set => SetLinkModeInternal(Handle, value); }

        public FbxCluster(IntPtr InHandle)
            : base(InHandle)
        {
        }

        public FbxCluster(FbxManager manager, string name)
            : this(CreateFromManager(manager.Handle, name))
        {
        }

        public FbxCluster(FbxObject obj, string name)
            : this(CreateFromObject(obj.Handle, name))
        {
        }

        public void SetLink(FbxNode node)
        {
            SetLinkInternal(Handle, node.Handle);
        }

        public FbxNode GetLink()
        {
            IntPtr ptr = GetLinkInternal(Handle);
            return new FbxNode(ptr);
        }

        public void SetLinkMode(ELinkMode mode)
        {
            SetLinkModeInternal(Handle, mode);
        }

        public void AddControlPointIndex(int index, double weight)
        {
            AddControlPointIndexInternal(Handle, index, weight);
        }

        public void SetTransformLinkMatrix(FbxAMatrix matrix)
        {
            SetTransformLinkMatrixInternal(Handle, matrix.Handle);
        }

        public int[]? GetControlPointIndices()
        {
            IntPtr ptr = GetControlPointIndicesInternal(Handle);
            if (ptr == IntPtr.Zero)
            {
                return null;
            }

            int[] outBuf = new int[ControlPointIndicesCount];
            Marshal.Copy(ptr, outBuf, 0, ControlPointIndicesCount);

            return outBuf;
        }

        public double[]? GetControlPointWeights()
        {
            IntPtr ptr = GetControlPointWeightsInternal(Handle);
            if (ptr == IntPtr.Zero)
            {
                return null;
            }

            double[] outBuf = new double[ControlPointIndicesCount];
            Marshal.Copy(ptr, outBuf, 0, ControlPointIndicesCount);

            return outBuf;
        }

        internal void SetTransformMatrix(FbxAMatrix pMatrix)
        {
            SetTransformMatrixInternal(pMatrix.Handle);
        }

    }

}
