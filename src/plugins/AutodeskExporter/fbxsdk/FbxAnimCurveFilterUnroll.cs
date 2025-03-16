using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxAnimCurveFilterUnroll : FbxObject
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "??0FbxAnimCurveFilterUnroll@fbxsdk@@QEAA@XZ")]
        private static extern void ConstructInternal(nint pHandle);
        [DllImport(NativeMethods.DllName, EntryPoint = "?Reset@FbxAnimCurveFilterUnroll@fbxsdk@@UEAAXXZ")]
        private static extern void ResetInternal(nint pHandle);
        [DllImport(NativeMethods.DllName, EntryPoint = "?SetQualityTolerance@FbxAnimCurveFilterUnroll@fbxsdk@@QEAAXN@Z")]
        private static extern void SetQualityToleranceInternal(nint pHandle, float filterPrecision);

        [DllImport(NativeMethods.DllName, EntryPoint = "?Apply@FbxAnimCurveFilterUnroll@fbxsdk@@UEAA_NPEAPEAVFbxAnimCurve@2@HPEAVFbxStatus@2@@Z")]
        private static extern void ApplyInternal(nint pHandle, nint dataPtr, int count);

        /// <summary>
        /// 占用的空间，使用 c++ 调用 sizeof(FbxMatrix)
        /// </summary>
        const ulong SizeOfThis = 0x30;

      


        public FbxAnimCurveFilterUnroll(nint InHandle)
            : base(InHandle)
        {
        }
        public FbxAnimCurveFilterUnroll()
            : base(FbxUtils.FbxMalloc(SizeOfThis))
        {
            ConstructInternal(Handle);
            _leaveFree = true;
        }

        internal void Reset()
        {
            ResetInternal(Handle);
        }


        internal void SetQualityTolerance(float filterPrecision)
        {
            SetQualityToleranceInternal(Handle, filterPrecision);
        }


        internal void Apply(FbxAnimCurve[] items)
        {
            var ptr = nint.Zero;
            ApplyInternal(Handle, ptr, items.Length);
        }

    }
}
