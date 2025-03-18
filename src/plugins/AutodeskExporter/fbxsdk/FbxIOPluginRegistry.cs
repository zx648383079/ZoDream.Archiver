using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxIOPluginRegistry : FbxNative
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?GetWriterFormatCount@FbxIOPluginRegistry@fbxsdk@@QEBAHXZ", CallingConvention = CallingConvention.ThisCall)]
        private static extern int GetWriterFormatCountInternal(nint InHandle);

        [DllImport(NativeMethods.DllName, EntryPoint = "?WriterIsFBX@FbxIOPluginRegistry@fbxsdk@@QEBA_NH@Z", CallingConvention = CallingConvention.ThisCall)]
        private static extern bool WriterIsFBXInternal(nint InHandle, int pFileFormat);

        [DllImport(NativeMethods.DllName, EntryPoint = "?GetWriterFormatDescription@FbxIOPluginRegistry@fbxsdk@@QEBAPEBDH@Z", CallingConvention = CallingConvention.ThisCall)]
        private static extern nint GetWriterFormatDescriptionInternal(nint InHandle, int pFileFormat);

        public FbxIOPluginRegistry(nint InHandle)
            : base(InHandle)
        {
        }

        public int WriterFormatCount => GetWriterFormatCountInternal(Handle);
        public bool WriterIsFBX(int pFileFormat) { return WriterIsFBXInternal(Handle, pFileFormat); }

        public string GetWriterFormatDescription(int pFileFormat)
        {
            nint Ptr = GetWriterFormatDescriptionInternal(Handle, pFileFormat);
            if (Ptr == nint.Zero)
                return "";

            return FbxUtils.IntPtrToString(Ptr);
        }
    }

}
