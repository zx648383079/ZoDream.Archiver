using System.Numerics;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxQuaternion : FbxNative
    {

        [DllImport(NativeMethods.DllName, EntryPoint = "??0FbxQuaternion@fbxsdk@@QEAA@XZ")]
        private static extern void ConstructInternal(nint handle);

        [DllImport(NativeMethods.DllName, EntryPoint = "??0FbxQuaternion@fbxsdk@@QEAA@NNNN@Z")]
        private static extern void ConstructInternal(nint handle, double pX, double pY, double pZ, double pW);
        [DllImport(NativeMethods.DllName, EntryPoint = "fbxsdk::FbxQuaternion::GetAt(int)")]
        private static extern double GetAtInternal(nint handle, int index);
        [DllImport(NativeMethods.DllName, EntryPoint = "?SetAt@FbxQuaternion@fbxsdk@@QEAAXHN@Z")]
        private static extern void SetAtInternal(nint handle, int index, double val);

        [DllImport(NativeMethods.DllName, EntryPoint = "?Set@FbxQuaternion@fbxsdk@@QEAAXNNNN@Z")]
        private static extern void SetInternal(nint handle, double pX, double pY, double pZ, double pW);
        /// <summary>
        /// 占用的空间，使用 c++ 调用 sizeof(FbxTime)
        /// </summary>
        const ulong SizeOfThis = 0x20;

        public FbxQuaternion(nint handle)
            : base(handle)
        {
        }

        public FbxQuaternion()
           : base(FbxUtils.FbxMalloc(SizeOfThis))
        {
            ConstructInternal(Handle);
            _leaveFree = true;
        }

        public FbxQuaternion(double pX, double pY, double pZ, double pW = 1.0)
            : base(FbxUtils.FbxMalloc(SizeOfThis))
        {
            ConstructInternal(Handle, pX, pY, pZ, pW);
            _leaveFree = true;
        }

        public FbxQuaternion(Quaternion q)
            : this(q.X, q.Y, q.Z, q.W)
        {
        }

        public FbxQuaternion(Vector4 q)
            : this(q.X, q.Y, q.Z, q.W)
        {
        }

        public void Set(double pX, double pY, double pZ, double pW)
        {
            SetInternal(Handle, pX, pY, pZ, pW);
        }

        public void Set(Quaternion q)
        {
            Set(q.X, q.Y, q.Z, q.W);
        }


        public double this[int index] {
            get => GetAtInternal(Handle, index);
            set => SetAtInternal(Handle, index, value);
        }


        public static explicit operator Quaternion(FbxQuaternion q)
        {
            return new((float)q[0], (float)q[1], (float)q[2], (float)q[3]);
        }

        public static explicit operator FbxQuaternion(Quaternion q)
        {
            return new(q);
        }

        public static explicit operator Vector4(FbxQuaternion q)
        {
            return new((float)q[0], (float)q[1], (float)q[2], (float)q[3]);
        }

        public static explicit operator FbxQuaternion(Vector4 q)
        {
            return new(q);
        }
    }
}
