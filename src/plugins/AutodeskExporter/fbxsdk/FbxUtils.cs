using System;
using System.Runtime.InteropServices;
using System.Text;

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
            NativeMethods.Ready();
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
            if (inPtr == IntPtr.Zero)
            {
                return string.Empty;
            }
            var ptr = (byte*)inPtr;
            var length = 0;
            while (ptr[length] != '\0')
            {
                length++;
            }
            var buffer = new byte[length];
            Marshal.Copy(inPtr, buffer, 0, length);
            return Encoding.ASCII.GetString(buffer);
        }

        public static unsafe string IntPtrToString(char* ptr, int length)
        {
            return new string(ptr, 0, length);
        }
    }
}
