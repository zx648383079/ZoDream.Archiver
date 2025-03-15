using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxMatrix : FbxNative
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "??0FbxMatrix@fbxsdk@@QEAA@AEBVFbxAMatrix@1@@Z")]
        private static extern void ConvertFromAffineInternal(IntPtr pHandle, IntPtr pMatrix);

        [DllImport(NativeMethods.DllName, EntryPoint = "?Get@FbxMatrix@fbxsdk@@QEBANHH@Z")]
        private static extern double GetInternal(IntPtr pHandle, int row, int column);

        public FbxMatrix(IntPtr ptr)
            : base(ptr)
        {
        }

        public FbxMatrix(FbxAMatrix affineMatrix)
            : base(FbxUtils.FbxMalloc(0x80))
        {
            ConvertFromAffineInternal(Handle, affineMatrix.Handle);
            _leaveFree = true;
        }

        public Matrix4x4 ToMatrix()
        {
            return new Matrix4x4(
                (float)GetInternal(Handle, 0, 0), (float)GetInternal(Handle, 0, 1), (float)GetInternal(Handle, 0, 2), (float)GetInternal(Handle, 0, 3),
                (float)GetInternal(Handle, 1, 0), (float)GetInternal(Handle, 1, 1), (float)GetInternal(Handle, 1, 2), (float)GetInternal(Handle, 1, 3),
                (float)GetInternal(Handle, 2, 0), (float)GetInternal(Handle, 2, 1), (float)GetInternal(Handle, 2, 2), (float)GetInternal(Handle, 2, 3),
                (float)GetInternal(Handle, 3, 0), (float)GetInternal(Handle, 3, 1), (float)GetInternal(Handle, 3, 2), (float)GetInternal(Handle, 3, 3)
                );
        }
    }
}
