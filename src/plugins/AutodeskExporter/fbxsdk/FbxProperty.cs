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
        [DllImport(NativeMethods.DllName, EntryPoint = "?Set@FbxProperty@fbxsdk@@IEAA_NPEBXAEBW4EFbxType@2@_N@Z")]
        private static extern void SetInternal(IntPtr InHandle, IntPtr pValue, ref EFbxType pValueType, bool pCheckForValueEquality);

        [DllImport(NativeMethods.DllName, EntryPoint = "?Get@FbxProperty@fbxsdk@@IEBA_NPEAXAEBW4EFbxType@2@@Z")]
        private static extern bool GetInternal(IntPtr InHandle, ref IntPtr pValue, ref EFbxType pValueType);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetCurve@FbxProperty@fbxsdk@@QEAAPEAVFbxAnimCurve@2@PEAVFbxAnimLayer@2@PEBD_N@Z")]
        private static extern nint GetCurveInternal(IntPtr InHandle, IntPtr pAnimLayer, [MarshalAs(UnmanagedType.LPStr)] string pChannel, bool pCreate = false);

        [DllImport(NativeMethods.DllName, EntryPoint = "?ConnectSrcObject@FbxProperty@fbxsdk@@QEAA_NPEAVFbxObject@2@W4EType@FbxConnection@2@@Z")]
        private static extern void ConnectSrcObjectInternal(nint inHandle, nint objHandle);


        public static void Set(IntPtr inHandle, string value)
        {
            EFbxType type = EFbxType.eFbxString;
            IntPtr ptr = FbxString.Construct(value);

            SetInternal(inHandle, ptr, ref type, true);
            FbxUtils.FbxFree(ptr);
        }

        public static void Set(IntPtr inHandle, Vector3 value)
        {
            EFbxType type = EFbxType.eFbxDouble3;
            IntPtr ptr = FbxDouble3.Construct(value);

            SetInternal(inHandle, ptr, ref type, true);
            FbxUtils.FbxFree(ptr);
        }

        public static unsafe void Set(IntPtr inHandle, double value)
        {
            EFbxType type = EFbxType.eFbxDouble;

            IntPtr ptr = FbxUtils.FbxMalloc(8);
            Marshal.WriteInt64(ptr, *((long*)&value));

            SetInternal(inHandle, ptr, ref type, true);
            FbxUtils.FbxFree(ptr);
        }

        public static unsafe void SetEnum(IntPtr inHandle, int value)
        {
            EFbxType type = EFbxType.eFbxEnum;
            IntPtr ptr = FbxUtils.FbxMalloc(4);
            Marshal.WriteInt32(ptr, *((int*)&value));

            SetInternal(inHandle, ptr, ref type, true);
            FbxUtils.FbxFree(ptr);
        }

        public static string GetString(IntPtr inHandle)
        {
            EFbxType type = EFbxType.eFbxString;
            IntPtr ptr = IntPtr.Zero;

            GetInternal(inHandle, ref ptr, ref type);
            return FbxUtils.IntPtrToString(ptr);
        }

        public static unsafe Vector3 GetDouble3(IntPtr inHandle)
        {
            EFbxType type = EFbxType.eFbxDouble3;
            IntPtr ptr = IntPtr.Zero;

            GetInternal(inHandle, ref ptr, ref type);
            return FbxDouble3.Get(new IntPtr((long*)&ptr));
        }

        public static unsafe double GetDouble(IntPtr inHandle)
        {
            EFbxType type = EFbxType.eFbxDouble;
            IntPtr ptr = IntPtr.Zero;

            GetInternal(inHandle, ref ptr, ref type);
            return *((double*)&ptr);
        }
        public static unsafe T GetEnum<T>(IntPtr inHandle)
            where T : struct, Enum
        {
            EFbxType type = EFbxType.eFbxEnum;
            IntPtr ptr = IntPtr.Zero;

            GetInternal(inHandle, ref ptr, ref type);
            var val = *((int*)&ptr);
            return (T)Enum.ToObject(typeof(T), val);
        }

        internal static FbxAnimCurve GetCurve(IntPtr inHandle, FbxAnimLayer pAnimLayer, string pChannel, bool pCreate = false)
        {
            var ptr = GetCurveInternal(inHandle, pAnimLayer.Handle, pChannel, pCreate);
            return new FbxAnimCurve(ptr);
        }

        internal static void ConnectSrcObject(IntPtr inHandle, FbxObject obj)
        {
            ConnectSrcObjectInternal(inHandle, obj.Handle);
        }

    }

}
