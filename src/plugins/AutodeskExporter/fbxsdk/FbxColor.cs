using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using ZoDream.Shared;

namespace ZoDream.AutodeskExporter
{
    internal class FbxColor : FbxNative
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "??0FbxColor@fbxsdk@@QEAA@NNNN@Z")]
        private static extern void ConstructInternal(nint handle, double pX, double pY, double pZ, double pW);
        [DllImport(NativeMethods.DllName, EntryPoint = "??0FbxColor@fbxsdk@@QEAA@XZ")]
        private static extern void ConstructInternal(nint handle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?Set@FbxColor@fbxsdk@@QEAAXNNNN@Z")]
        private static extern void SetInternal(nint handle, double pX, double pY, double pZ, double pW);

        /// <summary>
        /// 占用的空间，使用 c++ 调用 sizeof(FbxTime)
        /// </summary>
        internal const ulong SizeOfThis = 0x20;

        public FbxColor(nint handle)
            : base(handle)
        {
        }

        public FbxColor()
            : base(FbxUtils.FbxMalloc(SizeOfThis))
        {
            ConstructInternal(Handle);
            _leaveFree = true;
        }

        public FbxColor(double pX, double pY, double pZ, double pW = 1.0)
            : base(FbxUtils.FbxMalloc(SizeOfThis))
        {
            ConstructInternal(Handle, pX, pY, pZ, pW);
            _leaveFree = true;
        }

        public FbxColor(Vector4 value)
            : this(value.X, value.Y, value.Z, value.W)
        {
        }


        public unsafe double this[int index] {
            get {
                Expectation.ThrowIfNot(index is >= 0 and < 4);
                return *(double*)(Handle + (index * 8));
            }
            set {
                Expectation.ThrowIfNot(index is >= 0 and < 4);
                *(double*)(Handle + (index * 8)) = value;
            }
        }

        public void Set(double pX, double pY, double pZ, double pW)
        {
            SetInternal(Handle, pX, pY, pZ, pW);
        }

        public void Set(Vector4 q)
        {
            Set(q.X, q.Y, q.Z, q.W);
        }

        public static explicit operator Vector4(FbxColor q)
        {
            return new((float)q[0], (float)q[1], (float)q[2], (float)q[3]);
        }

        public static explicit operator FbxColor(Vector4 q)
        {
            return new(q);
        }

        public static explicit operator FbxColor(FbxDouble4 q)
        {
            return new(q.Handle);
        }
    }
}
