using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    /// <summary>
    /// 
    /// 获取属性地址偏移的方法  &((FbxNull*)0)->Look
    /// </summary>
    internal class FbxProperty
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?Set@FbxProperty@fbxsdk@@AEAA_NAEAPEBX@Z")]
        private static extern bool SetInternal(nint inHandle, nint pValue);

        [DllImport(NativeMethods.DllName, EntryPoint = "?Set@FbxProperty@fbxsdk@@AEAA_NAEAPEBX@Z")]
        private static extern bool SetInternal(nint inHandle, ref double pValue);

        [DllImport(NativeMethods.DllName, EntryPoint = "?Set@FbxProperty@fbxsdk@@AEAA_NAEAPEBX@Z")]
        private static extern bool SetInternal(nint inHandle, ref int pValue);

        [DllImport(NativeMethods.DllName, EntryPoint = "?Get@FbxProperty@fbxsdk@@IEBA_NPEAXAEBW4EFbxType@2@@Z")]
        private static extern bool GetInternal(nint inHandle, ref nint pValue, ref EFbxType pValueType);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetCurve@FbxProperty@fbxsdk@@QEAAPEAVFbxAnimCurve@2@PEAVFbxAnimLayer@2@PEBD_N@Z")]
        private static extern nint GetCurveInternal(nint inHandle, nint pAnimLayer, [MarshalAs(UnmanagedType.LPStr)] string pChannel, bool pCreate = false);

        [DllImport(NativeMethods.DllName, EntryPoint = "?ConnectSrcObject@FbxProperty@fbxsdk@@QEAA_NPEAVFbxObject@2@W4EType@FbxConnection@2@@Z")]
        private static extern void ConnectSrcObjectInternal(nint inHandle, nint objHandle);


        public static void Set(nint inHandle, string value)
        {
            var ptr = FbxString.Construct(value);
            SetInternal(inHandle, ptr);
            FbxUtils.FbxFree(ptr);
        }

        public static void Set(nint inHandle, Vector3 value)
        {
            // EFbxType type = EFbxType.eFbxDouble3;
            var ptr = FbxDouble3.Construct(value);
            SetInternal(inHandle, ptr);
            FbxUtils.FbxFree(ptr);
        }

        public static unsafe void Set(nint inHandle, double value)
        {
            SetInternal(inHandle, ref value);
        }

        public static unsafe void SetEnum(nint inHandle, int value)
        {
            SetInternal(inHandle, ref value);
        }

        public static string GetString(nint inHandle)
        {
            EFbxType type = EFbxType.eFbxString;
            var ptr = nint.Zero;

            GetInternal(inHandle, ref ptr, ref type);
            return FbxUtils.IntPtrToString(ptr);
        }

        public static unsafe Vector3 GetDouble3(nint inHandle)
        {
            EFbxType type = EFbxType.eFbxDouble3;
            var ptr = nint.Zero;

            GetInternal(inHandle, ref ptr, ref type);
            return FbxDouble3.Get(new nint((long*)&ptr));
        }

        public static unsafe double GetDouble(nint inHandle)
        {
            EFbxType type = EFbxType.eFbxDouble;
            var ptr = nint.Zero;

            GetInternal(inHandle, ref ptr, ref type);
            return *((double*)&ptr);
        }
        public static unsafe T GetEnum<T>(nint inHandle)
            where T : struct, Enum
        {
            EFbxType type = EFbxType.eFbxEnum;
            var ptr = nint.Zero;

            GetInternal(inHandle, ref ptr, ref type);
            var val = *((int*)&ptr);
            return (T)Enum.ToObject(typeof(T), val);
        }

        internal static FbxAnimCurve GetCurve(nint inHandle, FbxAnimLayer pAnimLayer, string pChannel, bool pCreate = false)
        {
            var ptr = GetCurveInternal(inHandle, pAnimLayer.Handle, pChannel, pCreate);
            return new FbxAnimCurve(ptr);
        }

        internal static void ConnectSrcObject(nint inHandle, FbxObject obj)
        {
            ConnectSrcObjectInternal(inHandle, obj.Handle);
        }

    }

}
