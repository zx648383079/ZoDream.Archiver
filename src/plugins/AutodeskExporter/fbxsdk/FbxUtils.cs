using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal static class FbxUtils
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?FbxMalloc@fbxsdk@@YAPEAX_K@Z")]
        private static extern IntPtr FbxMallocInternal(ulong size);

        [DllImport(NativeMethods.DllName, EntryPoint = "?FbxFree@fbxsdk@@YAXPEAX@Z")]
        private static extern void FbxFreeInternal(IntPtr ptr);

        public static IntPtr FbxMalloc(ulong size)
        {
            //return Marshal.AllocHGlobal((int)size);
            return FbxMallocInternal(size);
        }

        public static void FbxFree(IntPtr ptr)
        {
            // Marshal.FreeHGlobal(ptr);
            FbxFreeInternal(ptr);
        }

        public static unsafe string IntPtrToString(IntPtr inPtr)
        {
            string Str = "";
            byte* b = (byte*)inPtr;

            while ((*b) != 0x00)
            {
                Str += (char)(*b);
                b++;
            }

            return Str;
        }
    }
}
