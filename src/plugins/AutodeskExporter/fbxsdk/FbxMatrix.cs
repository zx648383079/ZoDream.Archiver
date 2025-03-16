using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxMatrix : FbxNative
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "??0FbxMatrix@fbxsdk@@QEAA@XZ")]
        private static extern void ConstructInternal(nint pHandle);

        [DllImport(NativeMethods.DllName, EntryPoint = "??0FbxMatrix@fbxsdk@@QEAA@AEBVFbxAMatrix@1@@Z")]
        private static extern void ConstructFromAffineInternal(nint pHandle, nint pMatrix);

        [DllImport(NativeMethods.DllName, EntryPoint = "?Get@FbxMatrix@fbxsdk@@QEBANHH@Z")]
        private static extern double GetInternal(nint pHandle, int row, int column);

        /// <summary>
        /// 占用的空间，使用 c++ 调用 sizeof(FbxMatrix)
        /// </summary>
        const ulong SizeOfThis = 0x80;

        public FbxMatrix(nint ptr)
            : base(ptr)
        {
        }

        public FbxMatrix(FbxAMatrix affineMatrix)
            : base(FbxUtils.FbxMalloc(SizeOfThis))
        {
            ConstructFromAffineInternal(Handle, affineMatrix.Handle);
            _leaveFree = true;
        }

        public Matrix4x4 ToMatrix()
        {
            var res = new Matrix4x4();
            for (var row = 0; row < 4; row++)
            {
                for (var column = 0; column < 4; column++)
                {
                    res[row, column] = (float)GetInternal(Handle, row, column);
                }
            }
            return res;
        }
    }
}
