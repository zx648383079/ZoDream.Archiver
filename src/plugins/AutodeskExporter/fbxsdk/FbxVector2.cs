using System.Numerics;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxVector2 : FbxDouble2
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "??0FbxVector2@fbxsdk@@QEAA@NN@Z")]
        private static extern void ConstructInternal(nint handle, double pX, double pY);
        [DllImport(NativeMethods.DllName, EntryPoint = "??0FbxVector2@fbxsdk@@QEAA@XZ")]
        private static extern void ConstructInternal(nint handle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?Set@FbxVector2@fbxsdk@@QEAAXNN@Z")]
        private static extern void SetInternal(nint handle, double pX, double pY);

        public FbxVector2(nint handle)
            : base(handle)
        {
        }

        public FbxVector2()
            : base(FbxUtils.FbxMalloc(SizeOfThis))
        {
            ConstructInternal(Handle);
            _leaveFree = true;
        }

        public FbxVector2(double pX, double pY)
            : base(FbxUtils.FbxMalloc(SizeOfThis))
        {
            ConstructInternal(Handle, pX, pY);
            _leaveFree = true;
        }

        public FbxVector2(Vector2 value)
            : this(value.X, value.Y)
        {
        }

        public void Set(double pX, double pY)
        {
            SetInternal(Handle, pX, pY);
        }

        public void Set(Vector2 q)
        {
            Set(q.X, q.Y);
        }


        public static explicit operator Vector2(FbxVector2 q)
        {
            return new((float)q[0], (float)q[1]);
        }

        public static explicit operator FbxVector2(Vector2 q)
        {
            return new(q);
        }

    }
}
