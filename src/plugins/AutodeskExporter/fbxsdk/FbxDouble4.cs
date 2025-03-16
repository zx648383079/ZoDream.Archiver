using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxDouble4
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "??0?$FbxVectorTemplate4@N@fbxsdk@@QEAA@NNNN@Z")]
        private static extern void ConstructInternal(nint handle, double pValue1, double pValue2, double pValue3, double pValue4);

        public static nint Construct(Vector4 value)
        {
            nint ptr = FbxUtils.FbxMalloc(8 * 4);
            ConstructInternal(ptr, value.X, value.Y, value.Z, value.W);
            return ptr;
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
