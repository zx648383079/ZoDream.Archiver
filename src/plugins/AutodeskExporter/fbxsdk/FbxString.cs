using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxString
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "??0FbxString@fbxsdk@@QEAA@PEBD@Z")]
        private static extern void ConstructInternal(nint Handle, [MarshalAs(UnmanagedType.LPStr)] string pParam);

        [DllImport(NativeMethods.DllName, EntryPoint = "??4FbxString@fbxsdk@@QEAAAEBV01@PEBD@Z")]
        private static extern void AssignInternal(nint Handle, [MarshalAs(UnmanagedType.LPStr)] string pParam);

        public static nint Construct(string InitialValue = "")
        {
            nint Ptr = FbxUtils.FbxMalloc(8);
            ConstructInternal(Ptr, InitialValue);
            return Ptr;
        }

        public static void Assign(nint InHandle, string pParam)
        {
            AssignInternal(InHandle, pParam);
        }

        public static unsafe string Get(nint inHandle)
        {
            nint ptr = new nint(*(long*)inHandle);
            return (ptr != nint.Zero) ? FbxUtils.IntPtrToString(ptr) : "";
        }
    }

}
