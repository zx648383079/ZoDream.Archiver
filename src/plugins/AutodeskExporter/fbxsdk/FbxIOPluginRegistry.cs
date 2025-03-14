using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxIOPluginRegistry : FbxNative
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?GetWriterFormatCount@FbxIOPluginRegistry@fbxsdk@@QEBAHXZ", CallingConvention = CallingConvention.ThisCall)]
        private static extern int GetWriterFormatCountInternal(IntPtr InHandle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?WriterIsFBX@FbxIOPluginRegistry@fbxsdk@@QEBA_NH@Z", CallingConvention = CallingConvention.ThisCall)]
        private static extern bool WriterIsFBXInternal(IntPtr InHandle, int pFileFormat);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetWriterFormatDescription@FbxIOPluginRegistry@fbxsdk@@QEBAPEBDH@Z", CallingConvention = CallingConvention.ThisCall)]
        private static extern IntPtr GetWriterFormatDescriptionInternal(IntPtr InHandle, int pFileFormat);

        public FbxIOPluginRegistry(IntPtr InHandle)
            : base(InHandle)
        {
        }

        public int WriterFormatCount => GetWriterFormatCountInternal(pHandle);
        public bool WriterIsFBX(int pFileFormat) { return WriterIsFBXInternal(pHandle, pFileFormat); }

        public string GetWriterFormatDescription(int pFileFormat)
        {
            IntPtr Ptr = GetWriterFormatDescriptionInternal(pHandle, pFileFormat);
            if (Ptr == IntPtr.Zero)
                return "";

            return FbxUtils.IntPtrToString(Ptr);
        }
    }

}
