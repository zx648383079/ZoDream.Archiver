using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxAMatrix : FbxNative
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "??0FbxAMatrix@fbxsdk@@QEAA@XZ")]
        private static extern void ConstructInternal(nint pHandle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?Inverse@FbxAMatrix@fbxsdk@@QEBA?AV12@XZ")]
        private static extern nint InverseInternal(nint pHandle);

        [DllImport(NativeMethods.DllName, EntryPoint = "??DFbxAMatrix@fbxsdk@@QEBA?AV01@AEBV01@@Z")]
        private static extern nint MultiplyInternal(nint pHandle, nint pOther);

        [DllImport(NativeMethods.DllName, EntryPoint = "?SetQ@FbxAMatrix@fbxsdk@@QEAAXAEBVFbxQuaternion@2@@Z")]
        private static extern void SetQInternal(nint handle, nint pQ);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetQ@FbxAMatrix@fbxsdk@@QEBA?AVFbxQuaternion@2@XZ")]
        private static extern nint GetQInternal(nint handle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetR@FbxAMatrix@fbxsdk@@QEBA?AVFbxVector4@2@XZ")]
        private static extern nint GetRInternal(nint handle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?SetR@FbxAMatrix@fbxsdk@@QEAAXAEBVFbxVector4@2@@Z")]
        private static extern void SetRInternal(nint handle, nint pR);


        /// <summary>
        /// 占用的空间，使用 c++ 调用 sizeof(FbxMatrix)
        /// </summary>
        const ulong SizeOfThis = 0x80;

        public FbxAMatrix(nint ptr)
            : base(ptr)
        {
        }

        public FbxAMatrix()
            : base(FbxUtils.FbxMalloc(SizeOfThis))
        {
            ConstructInternal(Handle);
            _leaveFree = true;
        }

        public FbxAMatrix(Matrix4x4 matrix)
            : this()
        {
            Set(matrix);
        }

        public Quaternion Q {
            get => FbxDouble4.Get(GetQInternal(Handle)).AsQuaternion();
            set {
                var ptr = FbxDouble4.Construct(value.AsVector4());
                SetQInternal(Handle, ptr);
                //FbxUtils.FbxFree(ptr);
            }
        }



        public Vector4 R {
            get => FbxDouble4.Get(GetRInternal(Handle));
            set {
                var ptr = FbxDouble4.Construct(value);
                SetRInternal(Handle, ptr);
                //FbxUtils.FbxFree(ptr);
            }
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
            Debug.Assert(ptr != nint.Zero);
            return new FbxAMatrix(ptr);
        }

        public static FbxAMatrix operator *(FbxAMatrix a, FbxAMatrix b)
        {
            var ptr = MultiplyInternal(a.Handle, b.Handle);
            Debug.Assert(ptr != nint.Zero);
            return new FbxAMatrix(ptr);
        }
    }

}
