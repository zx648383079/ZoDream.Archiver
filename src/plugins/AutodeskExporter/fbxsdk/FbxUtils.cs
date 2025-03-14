using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal static class FbxUtils
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?FbxMalloc@fbxsdk@@YAPEAX_K@Z")]
        private static extern IntPtr FbxMallocInternal(ulong Size);

        [DllImport(NativeMethods.DllName, EntryPoint = "?FbxFree@fbxsdk@@YAXPEAX@Z")]
        private static extern void FbxFreeInternal(IntPtr Ptr);

        public static IntPtr FbxMalloc(ulong Size)
        {
            return FbxMallocInternal(Size);
        }

        public static void FbxFree(IntPtr Ptr)
        {
            FbxFreeInternal(Ptr);
        }

        public static unsafe string IntPtrToString(IntPtr InPtr)
        {
            string Str = "";
            byte* b = (byte*)InPtr;

            while ((*b) != 0x00)
            {
                Str += (char)(*b);
                b++;
            }

            return Str;
        }
    }
}
