using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxDouble4 : FbxNative
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "??0?$FbxVectorTemplate4@N@fbxsdk@@QEAA@NNNN@Z")]
        private static extern void ConstructInternal(nint handle, double pValue1, double pValue2, double pValue3, double pValue4);

        /// <summary>
        /// 占用的空间，使用 c++ 调用 sizeof(FbxTime)
        /// </summary>
        internal const ulong SizeOfThis = 0x20;

        public FbxDouble4(nint handle)
            : base(handle)
        {
            
        }

        public FbxDouble4(Vector4 value)
            : this(value.X, value.Y, value.Z, value.W)
        {
        }

        public FbxDouble4(double pX, double pY, double pZ, double pW = 1.0)
            : base(FbxUtils.FbxMalloc(SizeOfThis))
        {
            ConstructInternal(Handle, pX, pY, pZ, pW);
            _leaveFree = true;
        }

        public unsafe double this[int index] {
            get {
                Debug.Assert(index is >= 0 and < 4);
                return *(double*)(Handle + (index * 8));
            }
            set {
                Debug.Assert(index is >= 0 and < 4);
                *(double*)(Handle + (index * 8)) = value;
            }
        }

        public static explicit operator Quaternion(FbxDouble4 q)
        {
            return new((float)q[0], (float)q[1], (float)q[2], (float)q[3]);
        }

        public static explicit operator FbxDouble4(Quaternion q)
        {
            return new(q.X, q.Y, q.Z, q.W);
        }

        public static explicit operator Vector4(FbxDouble4 q)
        {
            return new((float)q[0], (float)q[1], (float)q[2], (float)q[3]);
        }

        public static explicit operator FbxDouble4(Vector4 q)
        {
            return new(q);
        }

        public static nint Construct(Vector4 value)
        {
            nint ptr = FbxUtils.FbxMalloc(SizeOfThis);
            ConstructInternal(ptr, value.X, value.Y, value.Z, value.W);
            return ptr;
        }

        public static nint Construct(Vector3 value)
        {
            return Construct(new Vector4(value.X, value.Y, value.Z, 1));
        }

        public static unsafe Vector4 Get(nint inHandle)
        {
            float x = (float)*((double*)(inHandle.ToInt64()));
            float y = (float)*((double*)(inHandle.ToInt64() + 8));
            float z = (float)*((double*)(inHandle.ToInt64() + 16));
            float w = (float)*((double*)(inHandle.ToInt64() + 24));

            return new Vector4(x, y, z, w);
        }

    }

}
