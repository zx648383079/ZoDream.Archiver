using System.Numerics;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxVector4 : FbxDouble4
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "??0FbxVector4@fbxsdk@@QEAA@NNNN@Z")]
        private static extern void ConstructInternal(nint handle, double pX, double pY, double pZ, double pW);
        [DllImport(NativeMethods.DllName, EntryPoint = "??0FbxVector4@fbxsdk@@QEAA@XZ")]
        private static extern void ConstructInternal(nint handle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?Set@FbxVector4@fbxsdk@@QEAAXNNNN@Z")]
        private static extern void SetInternal(nint handle, double pX, double pY, double pZ, double pW);

        public FbxVector4(nint handle)
            : base(handle)
        {
        }

        public FbxVector4()
            : base(FbxUtils.FbxMalloc(SizeOfThis))
        {
            ConstructInternal(Handle);
            _leaveFree = true;
        }

        public FbxVector4(double pX, double pY, double pZ, double pW = 1.0)
            : base(FbxUtils.FbxMalloc(SizeOfThis))
        {
            ConstructInternal(Handle, pX, pY, pZ, pW);
            _leaveFree = true;
        }

        public FbxVector4(Vector4 value)
            : this(value.X, value.Y, value.Z, value.W)
        {
        }

        public void Set(double pX, double pY, double pZ, double pW)
        {
            SetInternal(Handle, pX, pY, pZ, pW);
        }

        public void Set(Vector4 q)
        {
            Set(q.X, q.Y, q.Z, q.W);
        }

        public static explicit operator Quaternion(FbxVector4 q)
        {
            return new((float)q[0], (float)q[1], (float)q[2], (float)q[3]);
        }

        public static explicit operator FbxVector4(Quaternion q)
        {
            return new(q.X, q.Y, q.Z, q.W);
        }

        public static explicit operator Vector4(FbxVector4 q)
        {
            return new((float)q[0], (float)q[1], (float)q[2], (float)q[3]);
        }

        public static explicit operator FbxVector4(Vector4 q)
        {
            return new(q);
        }

    }
}
