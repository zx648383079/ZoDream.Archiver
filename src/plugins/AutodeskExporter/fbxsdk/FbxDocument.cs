using System;
using System.Runtime.InteropServices;

namespace ZoDream.AutodeskExporter
{
    internal class FbxDocument : FbxCollection
    {
        [DllImport(NativeMethods.DllName, EntryPoint = "?GetDocumentInfo@FbxDocument@fbxsdk@@QEBAPEAVFbxDocumentInfo@2@XZ")]
        protected static extern nint GetDocumentInfoInternal(nint handle);

        public FbxDocument() { }
        public FbxDocument(nint InHandle)
            : base(InHandle)
        {
        }
    }

}
