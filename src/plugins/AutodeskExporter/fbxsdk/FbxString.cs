using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxString
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "??0FbxString@fbxsdk@@QEAA@PEBD@Z")]
        private static extern void ConstructInternal(IntPtr Handle, [MarshalAs(UnmanagedType.LPStr)] string pParam);

        [DllImport(NativeMethods.DllName, EntryPoint = "??4FbxString@fbxsdk@@QEAAAEBV01@PEBD@Z")]
        private static extern void AssignInternal(IntPtr Handle, [MarshalAs(UnmanagedType.LPStr)] string pParam);

        public static IntPtr Construct(string InitialValue = "")
        {
            IntPtr Ptr = FbxUtils.FbxMalloc(8);
            ConstructInternal(Ptr, InitialValue);
            return Ptr;
        }

        public static void Assign(IntPtr InHandle, string pParam)
        {
            AssignInternal(InHandle, pParam);
        }

        public static unsafe string Get(IntPtr InHandle)
        {
            IntPtr Ptr = new IntPtr(*(long*)InHandle);
            return (Ptr != IntPtr.Zero) ? FbxUtils.IntPtrToString(Ptr) : "";
        }
    }

}
