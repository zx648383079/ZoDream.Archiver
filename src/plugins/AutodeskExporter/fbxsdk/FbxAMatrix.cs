﻿using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxAMatrix : FbxNative
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "??0FbxAMatrix@fbxsdk@@QEAA@XZ")]
        private static extern nint ConstructInternal();

        [DllImport(NativeMethods.DllName, EntryPoint = "?Inverse@FbxAMatrix@fbxsdk@@QEBA?AV12@XZ")]
        private static extern nint InverseInternal(nint pHandle);

        [DllImport(NativeMethods.DllName, EntryPoint = "??DFbxAMatrix@fbxsdk@@QEBA?AV01@AEBV01@@Z")]
        private static extern nint MultiplyInternal(nint pHandle, nint pOther);
        public FbxAMatrix(IntPtr ptr)
            : base(ptr)
        {
        }

        public FbxAMatrix(Matrix4x4 matrix)
            : base(ConstructInternal())
        {
            Set(matrix);
        }

        public void Set(Matrix4x4 matrix)
        {
            var buffer = new byte[8 * 16];
            for (int i = 0; i < 4; i++)
            {
                for(int j = 0; j < 4; j++)
                {
                    var val = BitConverter.GetBytes((double)matrix[i, j]);
                    Array.Copy(val, 0, buffer, (i * 4 + j) * 8, val.Length);
                }
            }
            Marshal.Copy(Handle, buffer, 0, buffer.Length);
        }

        internal FbxAMatrix Inverse()
        {
            var ptr = InverseInternal(Handle);
            Debug.Assert(ptr != IntPtr.Zero);
            return new FbxAMatrix(ptr);
        }

        public static FbxAMatrix operator *(FbxAMatrix a, FbxAMatrix b)
        {
            var ptr = MultiplyInternal(a.Handle, b.Handle);
            Debug.Assert(ptr != IntPtr.Zero);
            return new FbxAMatrix(ptr);
        }
    }

}
