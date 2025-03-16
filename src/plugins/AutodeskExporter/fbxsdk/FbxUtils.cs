using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal static class FbxUtils
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?FbxMalloc@fbxsdk@@YAPEAX_K@Z")]
        private static extern nint FbxMallocInternal(ulong size);

        [DllImport(NativeMethods.DllName, EntryPoint = "?FbxFree@fbxsdk@@YAXPEAX@Z")]
        private static extern void FbxFreeInternal(nint ptr);

        public static nint FbxMalloc(ulong size)
        {
            //return Marshal.AllocHGlobal((int)size);
            return FbxMallocInternal(size);
        }

        public static void FbxFree(nint ptr)
        {
            // Marshal.FreeHGlobal(ptr);
            FbxFreeInternal(ptr);
        }

        public static unsafe string IntPtrToString(nint inPtr)
        {
            return Marshal.PtrToStringAuto(inPtr);
            //string Str = "";
            //byte* b = (byte*)inPtr;

            //while ((*b) != 0x00)
            //{
            //    Str += (char)(*b);
            //    b++;
            //}

            //return Str;
        }
    }
}
