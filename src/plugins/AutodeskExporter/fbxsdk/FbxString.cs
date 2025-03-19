using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxString : FbxNative
    {
        [DllImport(NativeMethods.DllName, CharSet = CharSet.Auto, EntryPoint = "??0FbxString@fbxsdk@@QEAA@PEBD@Z")]
        private static extern void ConstructInternal(nint handle, [MarshalAs(UnmanagedType.LPStr)] string pParam);
        [DllImport(NativeMethods.DllName, CharSet = CharSet.Auto, EntryPoint = "??0FbxString@fbxsdk@@QEAA@PEBD_K@Z")]
        private static extern void ConstructInternal(nint handle, nint pParam, ulong size);

        [DllImport(NativeMethods.DllName, CharSet = CharSet.Auto, EntryPoint = "??4FbxString@fbxsdk@@QEAAAEBV01@PEBD@Z")]
        private static extern void AssignInternal(nint handle, [MarshalAs(UnmanagedType.LPStr)] string pParam);
        [DllImport(NativeMethods.DllName, CharSet = CharSet.Auto, EntryPoint = "?AssignCopy@FbxString@fbxsdk@@AEAA_N_KPEBD@Z")]
        private static extern void AssignInternal(nint handle, ulong size, string pParam);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetLen@FbxString@fbxsdk@@QEBA_KXZ")]
        private static extern ulong LenInternal(nint handle);

        [DllImport(NativeMethods.DllName, CharSet = CharSet.Auto, EntryPoint = "??8FbxString@fbxsdk@@QEBA_NPEBD@Z")]
        private static extern bool CompareInternal(nint handle, [MarshalAs(UnmanagedType.LPStr)] string pParam);

        /// <summary>
        /// 占用的空间，使用 c++ 调用 sizeof(FbxTime)
        /// </summary>
        const ulong SizeOfThis = 0x8;

        public FbxString(nint handle)
            : base(handle)
        {
        }

        public unsafe FbxString(string value)
            : base(FbxUtils.FbxMalloc(SizeOfThis))
        {
            ConstructInternal(Handle, value);
            _leaveFree = true;
        }

        public override string ToString()
        {
            return Get(Handle);
        }

        public static explicit operator FbxString(string val)
        {
            return new(val);
        }

        public static explicit operator string(FbxString val)
        {
            return val.ToString();
        }

        public static nint Construct(string initialValue = "")
        {
            nint ptr = FbxUtils.FbxMalloc(SizeOfThis);
            ConstructInternal(ptr, initialValue);
            return ptr;
        }

        public static void Assign(nint InHandle, string pParam)
        {
            AssignInternal(InHandle, pParam);
        }

        public static unsafe string Get(nint inHandle)
        {
            return FbxUtils.IntPtrToString(*(nint*)inHandle);
        }

        public static bool Compare(nint namePtr, string value)
        {
            return CompareInternal(namePtr, value);
        }
    }

}
