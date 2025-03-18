using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxDouble3 : FbxNative
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "??0?$FbxVectorTemplate3@N@fbxsdk@@QEAA@NNN@Z")]
        private static extern void ConstructInternal(nint handle, double pValue1, double pValue2, double pValue3);


        /// <summary>
        /// 占用的空间，使用 c++ 调用 sizeof(FbxTime)
        /// </summary>
        const ulong SizeOfThis = 0x18;

        public FbxDouble3(nint handle)
            : base(handle)
        {
        }

        public FbxDouble3(Vector3 value)
            : base(FbxUtils.FbxMalloc(SizeOfThis))
        {
            ConstructInternal(Handle, value.X, value.Y, value.Z);
            _leaveFree = true;
        }


        public unsafe double this[int index] {
            get {
                Debug.Assert(index is >= 0 and < 3);
                return *(double*)(Handle + (index * 8));
            }
            set {
                Debug.Assert(index is >= 0 and < 3);
                *(double*)(Handle + (index * 8)) = value;
            }
        }

        public static explicit operator Vector3(FbxDouble3 q)
        {
            return new((float)q[0], (float)q[1], (float)q[2]);
        }

        public static explicit operator FbxDouble3(Vector3 q)
        {
            return new(q);
        }

        public static nint Construct(Vector3 value)
        {
            var ptr = FbxUtils.FbxMalloc(8 * 3);
            ConstructInternal(ptr, value.X, value.Y, value.Z);
            return ptr;
        }

        public static unsafe Vector3 Get(nint inHandle)
        {
            float x = (float)*((double*)(inHandle.ToInt64()));
            float y = (float)*((double*)(inHandle.ToInt64() + 8));
            float z = (float)*((double*)(inHandle.ToInt64() + 16));

            return new Vector3(x, y, z);
        }
    }

}
