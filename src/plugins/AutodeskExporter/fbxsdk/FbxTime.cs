using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxTime : FbxNative
    {
        public static FbxTime FBXSDK_TIME_INFINITE = new(0x7fffffffffffffff);
        const long FBXSDK_TC_MILLISECOND = 141120;

        [DllImport(NativeMethods.DllName, EntryPoint = "??0FbxTime@fbxsdk@@QEAA@_J@Z")]
        private static extern void ConstructInternal(nint inHandle, long pTime);
        [DllImport(NativeMethods.DllName, EntryPoint = "?SetSecondDouble@FbxTime@fbxsdk@@QEAAXN@Z")]
        private static extern void SetSecondDoubleInternal(nint pHandle, double value);

        /// <summary>
        /// 占用的空间，使用 c++ 调用 sizeof(FbxTime)
        /// </summary>
        internal const ulong SizeOfThis = 0x8;

        public FbxTime(long time)
            : base(FbxUtils.FbxMalloc(SizeOfThis))
        {
            ConstructInternal(Handle, time);
            _leaveFree = true;
        }

        public FbxTime(float time)
            : this((long)(time * 1000) * FBXSDK_TC_MILLISECOND)
        {

        }
    }

}
