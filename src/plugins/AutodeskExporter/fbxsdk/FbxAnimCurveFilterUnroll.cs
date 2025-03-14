using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxAnimCurveFilterUnroll : FbxObject
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "??1FbxAnimCurveFilterUnroll@fbxsdk@@UEAA@XZ")]
        private static extern IntPtr CreateFromObject();
        [DllImport(NativeMethods.DllName, EntryPoint = "?Reset@FbxAnimCurveFilterUnroll@fbxsdk@@UEAAXXZ")]
        private static extern void ResetInternal(nint pHandle);
        [DllImport(NativeMethods.DllName, EntryPoint = "?SetQualityTolerance@FbxAnimCurveFilterUnroll@fbxsdk@@QEAAXN@Z")]
        private static extern void SetQualityToleranceInternal(nint pHandle, float filterPrecision);

        [DllImport(NativeMethods.DllName, EntryPoint = "?Apply@FbxAnimCurveFilterUnroll@fbxsdk@@UEAA_NAEAVFbxAnimCurveNode@2@PEAVFbxStatus@2@@Z")]
        private static extern void ApplyInternal(nint pHandle, nint dataPtr, int count);
        internal void Reset()
        {
            ResetInternal(pHandle);
        }


        internal void SetQualityTolerance(float filterPrecision)
        {
            SetQualityToleranceInternal(pHandle, filterPrecision);
        }


        internal void Apply(FbxAnimCurve[] items)
        {
            var ptr = IntPtr.Zero;
            ApplyInternal(pHandle, ptr, items.Length);
        }



        public FbxAnimCurveFilterUnroll(IntPtr InHandle)
            : base(InHandle)
        {
        }
        public FbxAnimCurveFilterUnroll()
            : this(CreateFromObject())
        {
        }
    }
}
