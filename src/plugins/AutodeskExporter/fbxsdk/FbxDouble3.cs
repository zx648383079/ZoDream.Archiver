using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxDouble3
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "??0?$FbxVectorTemplate3@N@fbxsdk@@QEAA@NNN@Z")]
        private static extern void ConstructInternal(IntPtr handle, double pValue1, double pValue2, double pValue3);

        public static IntPtr Construct(Vector3 value)
        {
            var ptr = FbxUtils.FbxMalloc(8 * 3);
            ConstructInternal(ptr, value.X, value.Y, value.Z);
            return ptr;
        }

        public static unsafe Vector3 Get(IntPtr inHandle)
        {
            float x = (float)*((double*)(inHandle.ToInt64()));
            float y = (float)*((double*)(inHandle.ToInt64() + 8));
            float z = (float)*((double*)(inHandle.ToInt64() + 16));

            return new Vector3(x, y, z);
        }
    }

}
