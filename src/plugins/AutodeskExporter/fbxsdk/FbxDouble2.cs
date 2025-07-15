using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using ZoDream.Shared;

namespace ZoDream.AutodeskExporter
{
    internal class FbxDouble2 : FbxNative
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "??0?$FbxVectorTemplate2@N@fbxsdk@@QEAA@NN@Z")]
        private static extern void ConstructInternal(nint handle, double pValue1, double pValue2);


        /// <summary>
        /// 占用的空间，使用 c++ 调用 sizeof(FbxTime)
        /// </summary>
        internal const ulong SizeOfThis = 0x10;

        public FbxDouble2(nint handle)
            : base(handle)
        {
        }

        public FbxDouble2(double x, double y)
            : base(FbxUtils.FbxMalloc(SizeOfThis))
        {
            ConstructInternal(Handle, x, y);
            _leaveFree = true;
        }

        public FbxDouble2(Vector2 value)
            : this(value.X, value.Y)
        {
        }

        public unsafe double this[int index] {
            get {
                Expectation.ThrowIfNot(index is >= 0 and < 3);
                return *(double*)(Handle + (index * 8));
            }
            set {
                Expectation.ThrowIfNot(index is >= 0 and < 3);
                *(double*)(Handle + (index * 8)) = value;
            }
        }

        public static explicit operator Vector2(FbxDouble2 q)
        {
            return new((float)q[0], (float)q[1]);
        }

        public static explicit operator FbxDouble2(Vector2 q)
        {
            return new(q);
        }

        public static nint Construct(Vector2 value)
        {
            var ptr = FbxUtils.FbxMalloc(SizeOfThis);
            ConstructInternal(ptr, value.X, value.Y);
            return ptr;
        }

        public static unsafe Vector2 Get(nint inHandle)
        {
            float x = (float)*((double*)(inHandle.ToInt64()));
            float y = (float)*((double*)(inHandle.ToInt64() + 8));

            return new Vector2(x, y);
        }
    }
}
